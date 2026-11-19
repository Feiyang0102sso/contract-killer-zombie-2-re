"""Add an ad-hoc code signature to the linked main binary, with the original's identifier and entitlements (toolchain/entitlements.plist).

Why: ld only links; the original signature was added by codesign when Xcode packaged the app. No signature means no entitlements,
and iOS denies keychain access (syslog: `SecItemAdd: missing entitlement`). Every file StorageManager writes
stores a checksum in the keychain; if that fails it throws, DynamicContent fails, and "cannot connect to server" pops up.

Signature format matches ldid -S: SuperBlob = CodeDirectory (SHA-1, 4 KB pages) + empty Requirements + Entitlements.
The keychain access group and application-identifier match the original, so keychain data left by the original install (save checksums) stays readable.
Only the copy in bin/ is signed; build/tuned/ keeps ld's raw output for the L1/L2/L3 comparisons.

Usage: python toolchain/sign_adhoc.py <input> <output>
"""
import argparse
import hashlib
import logging
import struct
import sys
from pathlib import Path

# ---- Constants ----
TOOLCHAIN = Path(__file__).resolve().parent
# Exported verbatim from the original signature's Entitlements blob (the plist after the 8-byte blob header; keep the bytes exact)
ENTITLEMENTS_PLIST = TOOLCHAIN / "entitlements.plist"
# CodeDirectory identifier of the original
IDENTIFIER = b"com.glu.ckzombies2"
MH_MAGIC = 0xFEEDFACE
MACH_HEADER_SIZE = 28
LC_SEGMENT = 0x1
LC_CODE_SIGNATURE = 0x1D
LINKEDIT_SEGMENT = b"__LINKEDIT"
LINKEDIT_ALIGN = 0x1000
SIGNATURE_ALIGN = 16
# Code signature blobs (big-endian)
CSMAGIC_EMBEDDED_SIGNATURE = 0xFADE0CC0
CSMAGIC_CODEDIRECTORY = 0xFADE0C02
CSMAGIC_REQUIREMENTS = 0xFADE0C01
CSMAGIC_EMBEDDED_ENTITLEMENTS = 0xFADE7171
CSSLOT_CODEDIRECTORY = 0
CSSLOT_REQUIREMENTS = 2
CSSLOT_ENTITLEMENTS = 5
CODE_DIRECTORY_VERSION = 0x20100  # same as the original; the 48-byte header includes scatterOffset (0)
CODE_DIRECTORY_HEADER_SIZE = 48
SPECIAL_SLOT_COUNT = 5
HASH_TYPE_SHA1 = 1
HASH_SIZE = 20
PAGE_SIZE_LOG2 = 12
PAGE_SIZE = 1 << PAGE_SIZE_LOG2

log = logging.getLogger("sign_adhoc")


def align(value, alignment):
    """Round up to a multiple of alignment."""
    return (value + alignment - 1) // alignment * alignment


# ---- Entitlements and identifier (exported from the original signature) ----
def entitlements_blob():
    """Wrap entitlements.plist into an Entitlements blob: magic + length + the plist bytes, same as the original."""
    plist = ENTITLEMENTS_PLIST.read_bytes()
    return struct.pack(">II", CSMAGIC_EMBEDDED_ENTITLEMENTS, 8 + len(plist)) + plist


# ---- Build the signature ----
def requirements_blob():
    """Empty Requirements set, same as ldid."""
    return struct.pack(">III", CSMAGIC_REQUIREMENTS, 12, 0)


def code_directory(code, identifier, special_hashes):
    """CodeDirectory: special slot hashes (in reverse order before hashOffset) + per-page SHA-1."""
    page_count = (len(code) + PAGE_SIZE - 1) // PAGE_SIZE
    ident_offset = CODE_DIRECTORY_HEADER_SIZE
    ident = identifier + b"\0"
    hash_offset = ident_offset + len(ident) + SPECIAL_SLOT_COUNT * HASH_SIZE
    length = hash_offset + page_count * HASH_SIZE
    header = struct.pack(">9I4BII",
                         CSMAGIC_CODEDIRECTORY, length, CODE_DIRECTORY_VERSION, 0,
                         hash_offset, ident_offset, SPECIAL_SLOT_COUNT, page_count, len(code),
                         HASH_SIZE, HASH_TYPE_SHA1, 0, PAGE_SIZE_LOG2, 0, 0)
    parts = [header, ident]
    # Special slots -5 … -1 in order; slots without content are zero-filled
    for slot in range(SPECIAL_SLOT_COUNT, 0, -1):
        parts.append(special_hashes.get(slot, bytes(HASH_SIZE)))
    for page in range(page_count):
        parts.append(hashlib.sha1(code[page * PAGE_SIZE:(page + 1) * PAGE_SIZE]).digest())
    return b"".join(parts)


def code_directory_size(code_length, identifier):
    """The CodeDirectory length depends only on code length and identifier; compute it first to fix the signature size."""
    page_count = (code_length + PAGE_SIZE - 1) // PAGE_SIZE
    return CODE_DIRECTORY_HEADER_SIZE + len(identifier) + 1 + (SPECIAL_SLOT_COUNT + page_count) * HASH_SIZE


def superblob(blobs):
    """blobs: [(slot, bytes)] → EmbeddedSignature SuperBlob。"""
    header_size = 12 + len(blobs) * 8
    index = []
    offset = header_size
    for slot, blob in blobs:
        index.append(struct.pack(">II", slot, offset))
        offset += len(blob)
    parts = [struct.pack(">III", CSMAGIC_EMBEDDED_SIGNATURE, offset, len(blobs))]
    parts.extend(index)
    for slot, blob in blobs:
        parts.append(blob)
    return b"".join(parts)


def find_linkedit(data, ncmds):
    """Return the file offset of the __LINKEDIT segment command, and confirm the binary is not signed yet."""
    offset = MACH_HEADER_SIZE
    linkedit = None
    for index in range(ncmds):
        cmd, size = struct.unpack_from("<II", data, offset)
        if cmd == LC_CODE_SIGNATURE:
            raise SystemExit("binary is already signed")
        if cmd == LC_SEGMENT and data[offset + 8:offset + 24].rstrip(b"\0") == LINKEDIT_SEGMENT:
            linkedit = offset
        offset += size
    if linkedit is None:
        raise SystemExit("no __LINKEDIT segment")
    return linkedit


def sign(source, target):
    """Read source, add a signature and write it to target (they may be the same file)."""
    identifier = IDENTIFIER
    entitlements = entitlements_blob()
    data = bytearray(source.read_bytes())
    magic, cputype, cpusubtype, filetype, ncmds, sizeofcmds, flags = struct.unpack_from("<7I", data, 0)
    if magic != MH_MAGIC:
        raise SystemExit("not a thin 32-bit Mach-O: %s" % source)
    linkedit = find_linkedit(data, ncmds)

    # The new load command goes after the existing ones; that space must be empty
    command_offset = MACH_HEADER_SIZE + sizeofcmds
    if any(data[command_offset:command_offset + 16]):
        raise SystemExit("no free header space for LC_CODE_SIGNATURE")

    # The signature goes at the end of the file (16-byte aligned); fix the signature size first
    signature_offset = align(len(data), SIGNATURE_ALIGN)
    requirements = requirements_blob()
    blob_sizes = code_directory_size(signature_offset, identifier) + len(requirements) + len(entitlements)
    signature_size = align(12 + 3 * 8 + blob_sizes, SIGNATURE_ALIGN)

    # Patch the header: command count, command area size, LC_CODE_SIGNATURE, __LINKEDIT size
    struct.pack_into("<II", data, 16, ncmds + 1, sizeofcmds + 16)
    struct.pack_into("<4I", data, command_offset, LC_CODE_SIGNATURE, 16, signature_offset, signature_size)
    linkedit_fileoff = struct.unpack_from("<I", data, linkedit + 32)[0]
    linkedit_filesize = signature_offset + signature_size - linkedit_fileoff
    struct.pack_into("<I", data, linkedit + 28, align(linkedit_filesize, LINKEDIT_ALIGN))
    struct.pack_into("<I", data, linkedit + 36, linkedit_filesize)
    data.extend(bytes(signature_offset - len(data)))

    # With the header patched, hash the code area (everything before the signature) page by page
    special_hashes = {
        CSSLOT_REQUIREMENTS: hashlib.sha1(requirements).digest(),
        CSSLOT_ENTITLEMENTS: hashlib.sha1(entitlements).digest(),
    }
    directory = code_directory(bytes(data), identifier, special_hashes)
    signature = superblob([(CSSLOT_CODEDIRECTORY, directory),
                           (CSSLOT_REQUIREMENTS, requirements),
                           (CSSLOT_ENTITLEMENTS, entitlements)])
    if len(signature) > signature_size:
        raise SystemExit("signature size miscalculated")
    data.extend(signature)
    data.extend(bytes(signature_size - len(signature)))
    target.write_bytes(bytes(data))
    log.info("signed %s: identifier %s, %d pages, entitlements %d bytes",
             target.name, identifier.decode(), (signature_offset + PAGE_SIZE - 1) // PAGE_SIZE, len(entitlements))


def main():
    logging.basicConfig(level=logging.INFO, format="%(message)s")
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("source", type=Path)
    parser.add_argument("target", type=Path)
    args = parser.parse_args()
    sign(args.source, args.target)
    return 0


if __name__ == "__main__":
    sys.exit(main())
