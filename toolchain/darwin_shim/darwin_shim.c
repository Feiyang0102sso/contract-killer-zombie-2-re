/*
 * darwin_shim.c — Darwin functions missing when 32-bit maloader runs Unity 3.5.7 mono-xcompiler (i386 Mach-O).
 *
 * maloader resolves unknown symbols by name via dlsym(RTLD_DEFAULT), so injecting this library with LD_PRELOAD is enough; maloader needs no changes.
 * mono-xcompiler only does AOT compilation (single-threaded, GC_DONT_GC) and never needs real mach thread control; handled in five categories:
 *   1. Semaphores semaphore_*: needed by mono's thread synchronization; implemented for real with POSIX sem_t
 *   2. mach exception port thread (mach_port_* / mach_msg / exc_server): block that thread forever, without affecting the main thread
 *   3. GC stop-the-world, dyld image enumeration, etc.: should not be called under GC_DONT_GC; return empty results and log, so real calls are visible
 *   4. Condition variables __darwin_pthread_cond_*: Darwin i386 pthread_cond_t is only 28 bytes while glibc needs 48 bytes,
 *      so handing it straight to glibc overflows and treats the static-initializer signature as internal state (observed hanging in pthread_cond_broadcast).
 *      The maloader32 patch renames pthread_cond_* to these functions in rename.tab; the host's own pthread is unaffected.
 *   5. qsort: replaced with Mac libc's BSD quicksort (darwin_bsd_qsort), so equal keys are ordered as on Mac.
 *      When compiling Assembly-CSharp.dll with Mac mono + smcs, the order of same-target attributes in the CustomAttribute table depends on it.
 *
 * Build (inside ckre): gcc -m32 -shared -fPIC -O2 -o darwin_shim32.so darwin_shim.c -lpthread
 */
#define _GNU_SOURCE
#include <errno.h>
#include <pthread.h>
#include <semaphore.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/syscall.h>
#include <time.h>
#include <unistd.h>

/* ---- Constants (from Darwin mach/kern_return.h) ---- */
#define KERN_SUCCESS 0
#define KERN_FAILURE 5
#define KERN_OPERATION_TIMED_OUT 49

typedef unsigned int mach_port_t;
typedef int kern_return_t;

/* Darwin's mach_timespec_t: passed by value; ABI-identical to this struct on i386 */
typedef struct {
    unsigned int tv_sec;
    int tv_nsec;
} mach_timespec_t;

/* Data symbol exported by Darwin; glibc makes it a TLS macro, so a plain global variable is provided here */
int h_errno = 0;

static void shim_log(const char *name)
{
    fprintf(stderr, "[darwin_shim] unexpected call: %s\n", name);
}

/* ===================== 1. Semaphores ===================== */

/* semaphore_t is a 32-bit integer on 32-bit, just enough to hold a sem_t pointer */
kern_return_t semaphore_create(mach_port_t task, mach_port_t *semaphore, int policy, int value)
{
    sem_t *posix_semaphore = malloc(sizeof(sem_t));
    (void)task;
    (void)policy;
    if (posix_semaphore == NULL) {
        return KERN_FAILURE;
    }
    sem_init(posix_semaphore, 0, (unsigned int)value);
    *semaphore = (mach_port_t)(uintptr_t)posix_semaphore;
    return KERN_SUCCESS;
}

kern_return_t semaphore_destroy(mach_port_t task, mach_port_t semaphore)
{
    sem_t *posix_semaphore = (sem_t *)(uintptr_t)semaphore;
    (void)task;
    sem_destroy(posix_semaphore);
    free(posix_semaphore);
    return KERN_SUCCESS;
}

kern_return_t semaphore_signal(mach_port_t semaphore)
{
    sem_post((sem_t *)(uintptr_t)semaphore);
    return KERN_SUCCESS;
}

kern_return_t semaphore_wait(mach_port_t semaphore)
{
    sem_t *posix_semaphore = (sem_t *)(uintptr_t)semaphore;
    while (sem_wait(posix_semaphore) != 0) {
        if (errno != EINTR) {
            return KERN_FAILURE;
        }
    }
    return KERN_SUCCESS;
}

kern_return_t semaphore_timedwait(mach_port_t semaphore, mach_timespec_t wait_time)
{
    sem_t *posix_semaphore = (sem_t *)(uintptr_t)semaphore;
    struct timespec deadline;

    /* Darwin passes a relative time; sem_timedwait wants an absolute time */
    clock_gettime(CLOCK_REALTIME, &deadline);
    deadline.tv_sec += wait_time.tv_sec;
    deadline.tv_nsec += wait_time.tv_nsec;
    if (deadline.tv_nsec >= 1000000000) {
        deadline.tv_sec += 1;
        deadline.tv_nsec -= 1000000000;
    }
    while (sem_timedwait(posix_semaphore, &deadline) != 0) {
        if (errno == ETIMEDOUT) {
            return KERN_OPERATION_TIMED_OUT;
        }
        if (errno != EINTR) {
            return KERN_FAILURE;
        }
    }
    return KERN_SUCCESS;
}

/* ===================== 4. Condition variables ===================== */

/* Darwin i386 layout: long __sig; char __opaque[24]. __opaque is borrowed to store the pointer to the real glibc condition variable */
#define SHIM_COND_MAGIC 0x53484D43 /* 'SHMC': already replaced with a heap-allocated glibc condition variable */

struct darwin_cond {
    long signature;
    pthread_cond_t *real_cond;
};

static pthread_mutex_t cond_setup_lock = PTHREAD_MUTEX_INITIALIZER;

/* Allocate the glibc condition variable on first use; supports both static initialization (PTHREAD_COND_INITIALIZER) and pthread_cond_init */
static pthread_cond_t *real_cond_of(struct darwin_cond *cond)
{
    pthread_mutex_lock(&cond_setup_lock);
    if (cond->signature != SHIM_COND_MAGIC) {
        cond->real_cond = malloc(sizeof(pthread_cond_t));
        pthread_cond_init(cond->real_cond, NULL);
        cond->signature = SHIM_COND_MAGIC;
    }
    pthread_mutex_unlock(&cond_setup_lock);
    return cond->real_cond;
}

int __darwin_pthread_cond_init(struct darwin_cond *cond, const void *attributes)
{
    (void)attributes;
    cond->signature = 0;
    real_cond_of(cond);
    return 0;
}

int __darwin_pthread_cond_destroy(struct darwin_cond *cond)
{
    if (cond->signature == SHIM_COND_MAGIC) {
        pthread_cond_destroy(cond->real_cond);
        free(cond->real_cond);
    }
    cond->signature = 0;
    return 0;
}

int __darwin_pthread_cond_signal(struct darwin_cond *cond)
{
    return pthread_cond_signal(real_cond_of(cond));
}

int __darwin_pthread_cond_broadcast(struct darwin_cond *cond)
{
    return pthread_cond_broadcast(real_cond_of(cond));
}

/* Mutexes follow maloader's approach: Darwin mutex memory is used directly as a glibc mutex (44 bytes >= 24 bytes) */
int __darwin_pthread_cond_wait(struct darwin_cond *cond, pthread_mutex_t *mutex)
{
    return pthread_cond_wait(real_cond_of(cond), mutex);
}

int __darwin_pthread_cond_timedwait(struct darwin_cond *cond, pthread_mutex_t *mutex, const struct timespec *deadline)
{
    return pthread_cond_timedwait(real_cond_of(cond), mutex, deadline);
}

/* ===================== 2. mach exception port thread ===================== */

static mach_port_t next_port_name = 0x1000;

kern_return_t mach_port_allocate(mach_port_t task, int right, mach_port_t *name)
{
    (void)task;
    (void)right;
    next_port_name += 1;
    *name = next_port_name;
    return KERN_SUCCESS;
}

kern_return_t mach_port_insert_right(mach_port_t task, mach_port_t name, mach_port_t poly, int poly_type)
{
    (void)task;
    (void)name;
    (void)poly;
    (void)poly_type;
    return KERN_SUCCESS;
}

/* Only mono's exception-handling thread receives messages: block it forever, equivalent to "no exception ever happens" */
kern_return_t mach_msg(void *message, int option, unsigned int send_size, unsigned int receive_limit,
                       mach_port_t receive_name, unsigned int timeout, mach_port_t notify)
{
    (void)message;
    (void)option;
    (void)send_size;
    (void)receive_limit;
    (void)receive_name;
    (void)timeout;
    (void)notify;
    for (;;) {
        pause();
    }
    return KERN_FAILURE;
}

int exc_server(void *request, void *reply)
{
    (void)request;
    (void)reply;
    shim_log("exc_server");
    return 0;
}

mach_port_t mach_thread_self(void)
{
    return (mach_port_t)syscall(SYS_gettid);
}

/* ===================== 3. Thread stack, GC, dyld ===================== */

/* Darwin returns the stack top (high address); under maloader, pthread_t is glibc's pthread_t */
void *pthread_get_stackaddr_np(pthread_t thread)
{
    pthread_attr_t attributes;
    void *stack_low = NULL;
    size_t stack_size = 0;
    pthread_getattr_np(thread, &attributes);
    pthread_attr_getstack(&attributes, &stack_low, &stack_size);
    pthread_attr_destroy(&attributes);
    return (char *)stack_low + stack_size;
}

size_t pthread_get_stacksize_np(pthread_t thread)
{
    pthread_attr_t attributes;
    void *stack_low = NULL;
    size_t stack_size = 0;
    pthread_getattr_np(thread, &attributes);
    pthread_attr_getstack(&attributes, &stack_low, &stack_size);
    pthread_attr_destroy(&attributes);
    return stack_size;
}

/* Instruction and data caches are coherent on x86; nothing to do */
void sys_icache_invalidate(void *start, size_t length)
{
    (void)start;
    (void)length;
}

/* Used by GC stop-the-world; should not be reached under GC_DONT_GC */
kern_return_t task_threads(mach_port_t task, mach_port_t **threads, unsigned int *count)
{
    (void)task;
    shim_log("task_threads");
    *threads = NULL;
    *count = 0;
    return KERN_FAILURE;
}

kern_return_t thread_suspend(mach_port_t thread)
{
    (void)thread;
    shim_log("thread_suspend");
    return KERN_FAILURE;
}

kern_return_t thread_resume(mach_port_t thread)
{
    (void)thread;
    shim_log("thread_resume");
    return KERN_FAILURE;
}

kern_return_t thread_get_state(mach_port_t thread, int flavor, void *state, unsigned int *count)
{
    (void)thread;
    (void)flavor;
    (void)state;
    (void)count;
    shim_log("thread_get_state");
    return KERN_FAILURE;
}

kern_return_t thread_info(mach_port_t thread, int flavor, void *info, unsigned int *count)
{
    (void)thread;
    (void)flavor;
    (void)info;
    (void)count;
    shim_log("thread_info");
    return KERN_FAILURE;
}

/* dyld image enumeration: report "no loaded images". The GC therefore does not scan static data roots, which is harmless under GC_DONT_GC */
unsigned int _dyld_image_count(void)
{
    return 0;
}

const void *_dyld_get_image_header(unsigned int index)
{
    (void)index;
    return NULL;
}

const char *_dyld_get_image_name(unsigned int index)
{
    (void)index;
    return NULL;
}

intptr_t _dyld_get_image_vmaddr_slide(unsigned int index)
{
    (void)index;
    return 0;
}

void _dyld_register_func_for_add_image(void (*callback)(const void *header, intptr_t slide))
{
    (void)callback;
}

void _dyld_register_func_for_remove_image(void (*callback)(const void *header, intptr_t slide))
{
    (void)callback;
}

/* The Darwin prototype returns bool; callers check the return value and abort on failure */
int _dyld_bind_fully_image_containing_address(const void *address)
{
    (void)address;
    return 1;
}

const void *getsectbynamefromheader(const void *header, const char *segment, const char *section)
{
    (void)header;
    (void)segment;
    (void)section;
    return NULL;
}

unsigned long get_end(void)
{
    return 0;
}

unsigned long get_etext(void)
{
    return 0;
}

int proc_name(int pid, void *buffer, unsigned int size)
{
    (void)pid;
    (void)buffer;
    (void)size;
    return 0;
}

/* ===================== 5. BSD qsort ===================== */
/*
 * Mac libc's qsort is the FreeBSD port of the Bentley & McIlroy quicksort (unstable); glibc's qsort is a merge sort (stable).
 * The final order of equal elements depends on the algorithm itself: mono's Reflection.Emit qsorts the CustomAttribute table by Parent only,
 * so the order of multiple attributes on the same target is whatever Mac qsort produces. The maloader32 patch renames the Mach-O program's
 * qsort to darwin_bsd_qsort in rename.tab; the host's own qsort is unaffected.
 * The algorithm is transcribed from the disassembly of _qsort/__qsort in libsystem_sim_c.dylib of Xcode 4.5 iPhoneSimulator6.0.sdk
 * (same source as the contemporary OS X 10.8 Libc). It differs from the FreeBSD version in three ways, all affecting the order of equal elements:
 *   1. Depth limit 2*(fls(n)-1), decremented on each pivot selection; heapsort is used once it is exhausted;
 *   2. Both vecswaps are done first, then the "no swap at all during partitioning" check;
 *   3. The insertion sort at that point does at most 1 + n/4 swaps and gives up beyond that (nevermind), returning to the recursive quicksort part.
 * n < 7: insertion sort; n > 7: median of three; n > 40: median of nine; three-way partition; recurse into the left part first, loop instead of recursing on the right part.
 * The swap method (by long or by byte) only affects speed, not the result.
 */
typedef int (*bsd_cmp_t)(const void *, const void *);

/* Swap two elements, byte by byte */
static void bsd_swap_bytes(char *a, char *b, size_t n)
{
    size_t i;
    char t;
    for (i = 0; i < n; i++) {
        t = a[i];
        a[i] = b[i];
        b[i] = t;
    }
}

/* Swap the n bytes starting at a with the n bytes starting at b as a block (vecswap) */
static void bsd_vecswap(char *a, char *b, size_t n)
{
    if (n > 0) {
        bsd_swap_bytes(a, b, n);
    }
}

/* Median of three, comparing in the same order as the original implementation (the number of comparisons affects the result; do not rewrite it another way) */
static char *bsd_med3(char *a, char *b, char *c, bsd_cmp_t cmp)
{
    if (cmp(a, b) < 0) {
        if (cmp(b, c) < 0) {
            return b;
        }
        if (cmp(a, c) < 0) {
            return c;
        }
        return a;
    }
    if (cmp(b, c) > 0) {
        return b;
    }
    if (cmp(a, c) < 0) {
        return a;
    }
    return c;
}

/* Insertion sort: starting from the second element, swap each one backward into place */
static void bsd_insertion_sort(char *base, size_t n, size_t es, bsd_cmp_t cmp)
{
    char *pm;
    char *pl;
    for (pm = base + es; pm < base + n * es; pm += es) {
        for (pl = pm; pl > base && cmp(pl - es, pl) > 0; pl -= es) {
            bsd_swap_bytes(pl, pl - es, es);
        }
    }
}

/* Insertion sort with a swap limit; returns 0 if the limit is exceeded (the array is already partially modified), 1 once sorted */
static int bsd_insertion_sort_limited(char *base, size_t n, size_t es, bsd_cmp_t cmp, size_t limit)
{
    char *pm;
    char *pl;
    size_t swaps = 0;
    for (pm = base + es; pm < base + n * es; pm += es) {
        for (pl = pm; pl > base && cmp(pl - es, pl) > 0; pl -= es) {
            bsd_swap_bytes(pl, pl - es, es);
            swaps++;
            if (swaps > limit) {
                return 0;
            }
        }
    }
    return 1;
}

/* FreeBSD heapsort.c: elements are numbered 1..n; k temporarily holds the displaced element */
static void bsd_heapsort(char *vbase, size_t nmemb, size_t es, bsd_cmp_t cmp)
{
    char *base;
    char *k;
    char *child;
    char *par;
    size_t l;
    size_t par_i;
    size_t child_i;

    if (nmemb <= 1) {
        return;
    }
    k = malloc(es);
    if (k == NULL) {
        return;
    }
    base = vbase - es;

    /* Build the heap */
    for (l = nmemb / 2 + 1; --l;) {
        for (par_i = l; (child_i = par_i * 2) <= nmemb; par_i = child_i) {
            child = base + child_i * es;
            if (child_i < nmemb && cmp(child, child + es) < 0) {
                child += es;
                ++child_i;
            }
            par = base + par_i * es;
            if (cmp(child, par) <= 0) {
                break;
            }
            bsd_swap_bytes(par, child, es);
        }
    }

    /* Repeatedly move the heap top to the end, then sift the displaced element down into place */
    while (nmemb > 1) {
        memcpy(k, base + nmemb * es, es);
        memcpy(base + nmemb * es, base + es, es);
        --nmemb;
        for (par_i = 1; (child_i = par_i * 2) <= nmemb; par_i = child_i) {
            child = base + child_i * es;
            if (child_i < nmemb && cmp(child, child + es) < 0) {
                child += es;
                ++child_i;
            }
            par = base + par_i * es;
            memcpy(par, child, es);
        }
        for (;;) {
            child_i = par_i;
            par_i = child_i / 2;
            child = base + child_i * es;
            par = base + par_i * es;
            if (child_i == 1 || cmp(k, par) < 0) {
                memcpy(child, k, es);
                break;
            }
            memcpy(child, par, es);
        }
    }
    free(k);
}

static void bsd_qsort_depth(void *a, size_t n, size_t es, bsd_cmp_t cmp, int depth);

void darwin_bsd_qsort(void *a, size_t n, size_t es, bsd_cmp_t cmp)
{
    int depth = 0;
    size_t value = n;
    /* fls(n): index of the highest set bit (1-based); 0 when n is 0 */
    while (value != 0) {
        depth++;
        value >>= 1;
    }
    bsd_qsort_depth(a, n, es, cmp, 2 * depth - 2);
}

static void bsd_qsort_depth(void *a, size_t n, size_t es, bsd_cmp_t cmp, int depth)
{
    char *base;
    char *pa;
    char *pb;
    char *pc;
    char *pd;
    char *pl;
    char *pm;
    char *pn;
    size_t d;
    size_t r;
    int cmp_result;
    int swap_cnt;

loop:
    base = (char *)a;
    /* Depth exhausted: switch to heapsort */
    if (depth <= 0) {
        bsd_heapsort(base, n, es, cmp);
        return;
    }
    if (n < 7) {
        bsd_insertion_sort(base, n, es, cmp);
        return;
    }
    pm = base + (n / 2) * es;
    if (n > 7) {
        pl = base;
        pn = base + (n - 1) * es;
        if (n > 40) {
            d = (n / 8) * es;
            pl = bsd_med3(pl, pl + d, pl + 2 * d, cmp);
            pm = bsd_med3(pm - d, pm, pm + d, cmp);
            pn = bsd_med3(pn - 2 * d, pn - d, pn, cmp);
        }
        pm = bsd_med3(pl, pm, pn, cmp);
    }
    /* Decrement depth after choosing the pivot (disassembly 0x4a0a4) */
    depth--;
    swap_cnt = 0;
    bsd_swap_bytes(base, pm, es);
    pa = base + es;
    pb = base + es;

    pc = base + (n - 1) * es;
    pd = base + (n - 1) * es;
    for (;;) {
        while (pb <= pc) {
            cmp_result = cmp(pb, base);
            if (cmp_result > 0) {
                break;
            }
            if (cmp_result == 0) {
                swap_cnt = 1;
                bsd_swap_bytes(pa, pb, es);
                pa += es;
            }
            pb += es;
        }
        while (pb <= pc) {
            cmp_result = cmp(pc, base);
            if (cmp_result < 0) {
                break;
            }
            if (cmp_result == 0) {
                swap_cnt = 1;
                bsd_swap_bytes(pc, pd, es);
                pd -= es;
            }
            pc -= es;
        }
        if (pb > pc) {
            break;
        }
        bsd_swap_bytes(pb, pc, es);
        swap_cnt = 1;
        pb += es;
        pc -= es;
    }
    pn = base + n * es;
    r = (size_t)(pa - base);
    if ((size_t)(pb - pa) < r) {
        r = (size_t)(pb - pa);
    }
    bsd_vecswap(base, pb - r, r);
    r = (size_t)(pd - pc);
    if ((size_t)(pn - pd) - es < r) {
        r = (size_t)(pn - pd) - es;
    }
    bsd_vecswap(pb, pn - r, r);

    /* No swaps during partitioning: try insertion sort first, give up after more than 1 + n/4 swaps and continue with the recursion (nevermind) */
    if (swap_cnt == 0) {
        if (bsd_insertion_sort_limited(base, n, es, cmp, 1 + n / 4)) {
            return;
        }
    }

    r = (size_t)(pb - pa);
    if (r > es) {
        bsd_qsort_depth(base, r / es, es, cmp, depth);
    }
    r = (size_t)(pd - pc);
    if (r > es) {
        /* Loop instead of recursing on the right part */
        a = pn - r;
        n = r / es;
        goto loop;
    }
}
