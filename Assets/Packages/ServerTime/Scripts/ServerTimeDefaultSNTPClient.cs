using System;
using System.Net;
using System.Net.Sockets;

internal class ServerTimeDefaultSNTPClient : ServerTime.SNTPClientInterface
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.ServerTime.SNTPClient");
		}
	}

	private const byte SNTP_DATA_LENGTH = 48;

	private const byte TRANSMIT_OFFSET = 40;

	private const int TIMEOUT = 5000;

	private byte[] SNTPData = new byte[48];

	public DateTime GetServerTime()
	{
		DateTime result = DateTime.MinValue;
		try
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(GetServerURL());
			if (hostEntry != null && hostEntry.AddressList != null && hostEntry.AddressList.Length > 0 && hostEntry.AddressList[0] != null)
			{
				IPEndPoint remoteEP = new IPEndPoint(hostEntry.AddressList[0], 123);
				UdpClient udpClient = new UdpClient();
				udpClient.Client.SendTimeout = 5000;
				udpClient.Client.ReceiveTimeout = 5000;
				InitSNTPDataStruct();
				udpClient.Send(SNTPData, SNTPData.Length, remoteEP);
				SNTPData = udpClient.Receive(ref remoteEP);
				if (IsResponseModeValid())
				{
					result = MillisToDateTime(GetMillisByOffset(40));
				}
				else
				{
				}
				udpClient.Close();
			}
		}
		catch (Exception e)
		{
		}
		return result;
	}

	private DateTime MillisToDateTime(ulong millis)
	{
		TimeSpan timeSpan = TimeSpan.FromMilliseconds(millis);
		DateTime dateTime = new DateTime(1900, 1, 1);
		dateTime += timeSpan;
		return dateTime;
	}

	private string GetServerURL()
	{
		return "pool.ntp.org";
	}

	private void InitSNTPDataStruct()
	{
		SNTPData[0] = 27;
		for (int i = 1; i < SNTPData.Length; i++)
		{
			SNTPData[i] = 0;
		}
	}

	private bool IsResponseModeValid()
	{
		if (SNTPData == null)
		{
			return false;
		}
		if (SNTPData.Length < 48)
		{
			SNTPData = new byte[48];
			return false;
		}
		byte b = (byte)(SNTPData[0] & 7);
		return b == 4;
	}

	private ulong GetMillisByOffset(byte offset)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		for (int i = 0; i <= 3; i++)
		{
			num = 256 * num + SNTPData[offset + i];
		}
		for (int j = 4; j <= 7; j++)
		{
			num2 = 256 * num2 + SNTPData[offset + j];
		}
		return num * 1000 + num2 * 1000 / 4294967296L;
	}
}
