﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace K8055Velleman;

static public class K8055
{
	private class Interface
	{
		private const string PathToK8055 = "Resources\\K8055D.dll";
		#region k08055
		[DllImport(PathToK8055)]
		public static extern int OpenDevice(int CardAddress);
		[DllImport(PathToK8055)]
		public static extern void CloseDevice();
		[DllImport(PathToK8055)]
		public static extern int ReadAnalogChannel(int Channel);
		[DllImport(PathToK8055)]
		public static extern void ReadAllAnalog(ref int Data1, ref int Data2);
		[DllImport(PathToK8055)]
		public static extern void OutputAnalogChannel(int Channel, int Data);
		[DllImport(PathToK8055)]
		public static extern void OutputAllAnalog(int Data1, int Data2);
		[DllImport(PathToK8055)]
		public static extern void ClearAnalogChannel(int Channel);
		[DllImport(PathToK8055)]
		public static extern void SetAllAnalog();
		[DllImport(PathToK8055)]
		public static extern void ClearAllAnalog();
		[DllImport(PathToK8055)]
		public static extern void SetAnalogChannel(int Channel);
		[DllImport(PathToK8055)]
		public static extern void WriteAllDigital(int Data);
		[DllImport(PathToK8055)]
		public static extern void ClearDigitalChannel(int Channel);
		[DllImport(PathToK8055)]
		public static extern void ClearAllDigital();
		[DllImport(PathToK8055)]
		public static extern void SetDigitalChannel(int Channel);

		[DllImport(PathToK8055)]
		public static extern void SetAllDigital();
		[DllImport(PathToK8055)]
		public static extern bool ReadDigitalChannel(int Channel);
		[DllImport(PathToK8055)]
		public static extern int ReadAllDigital();
		[DllImport(PathToK8055)]
		public static extern int ReadCounter(int CounterNr);
		[DllImport(PathToK8055)]
		public static extern void ResetCounter(int CounterNr);
		[DllImport(PathToK8055)]
		public static extern void SetCounterDebounceTime(int CounterNr, int DebounceTime);
		[DllImport(PathToK8055)]
		public static extern int Version();
		[DllImport(PathToK8055)]
		public static extern int SearchDevices();
		[DllImport(PathToK8055)]
		public static extern int SetCurrentDevice(int lngCardAddress);
		#endregion
	}

	public enum DigitalChannel : int
	{
		L1 = -1,
		L2 = -2,
		L3 = -3,
		L4 = -4,
		L5 = -5,
		L6 = -6,
		L7 = -7,
		L8 = -8,
		B1 = 1,
		B2 = 2,
		B3 = 3,
		B4 = 4,
		B5 = 5, 
	}

    public delegate void onConnectionChanged();
    public static event onConnectionChanged OnConnectionChanged;

    public delegate void onDigitalChannelsChange(DigitalChannel digitalChannel);
    public static event onDigitalChannelsChange OnDigitalChannelsChange;

    public static readonly List<int> ConnectedDevices = [];

	public static bool IsConnected { get { return ConnectedDevices.Contains(CurrentDevice); } }
	
	public static int CurrentDevice { get; private set; } = -1;//IsConnected ? CurrentDevice : -1;

	private static readonly Dictionary<int, List<DigitalChannel>> digitalChannels = [];

	public static void Update()
	{
		bool oldIsConnected = IsConnected;
        UpdateConnectedDevice();
        if (!IsConnected)
		{
		SearchAndOpenDevice();
			if(!IsConnected) return;
		}

		if (IsConnected != oldIsConnected)
		{
			OnConnectionChanged?.Invoke();
        }

		UpdateDigitalsChannel();

        //if((digitalChannel & 1) > 0)
    }

	public static int OpenDevice(int CardAddress)
	{
		if(ConnectedDevices.Contains(CardAddress)) return SetCurrentDevice(CardAddress);
		CurrentDevice = Interface.OpenDevice(CardAddress);

		if (CurrentDevice >= 0)
		{
			ConnectedDevices.Add(CurrentDevice);
			Reset();
		}
		return CurrentDevice;
        }

	public static bool CloseDevice(int CardAddress) 
	{
		int tempCurrentDevice = CurrentDevice;
		if (CardAddress >= 0 && ConnectedDevices.Contains(CardAddress))
		{
			if(SetCurrentDevice(CardAddress) >= 0)
			{
				Reset();
				CloseDevice();
				SetCurrentDevice(tempCurrentDevice);
				return true;
			}
		}
		return false;
	}

	public static void CloseDevice()
	{
		if(IsConnected)
		{
			Reset();
			Interface.CloseDevice();
			ConnectedDevices.Remove(CurrentDevice);
			CurrentDevice = -1;
		}
	}

	public static void CloseAllDevices()
	{
		List<int> temp = new(ConnectedDevices);
		foreach(int CardAddress in temp)
		{
			if(SetCurrentDevice(CardAddress) >= 0)
			{
				Reset();
				CloseDevice();
			}
		}
	}

	public static int SetCurrentDevice(int CardAddress)
	{
		if(Interface.SetCurrentDevice(CardAddress) >= 0)
		{
			CurrentDevice = CardAddress;
			return CurrentDevice;
		}
		return -1;
	}

	public static List<int> GetAvailableDevices()
	{
		List<int> availableDevices = [];
		int i = Interface.SearchDevices();
		if(i - 8 >= 0)
		{
			i -= 8;
			availableDevices.Add(3);
		}
		if(i - 4 >= 0)
		{
			i -= 4;
			availableDevices.Add(2);
		}
		if(i - 2 >= 0)
		{
			i -= 2;
			availableDevices.Add(1);
		}
		if (i - 1 >= 0)
		{
			availableDevices.Add(0);
		}

		return availableDevices;

	}

	public static int SearchAndOpenDevice()
	{
		List<int> availibleDevice = GetAvailableDevices();

		foreach(int device in availibleDevice)
		{
			int i = OpenDevice(device);
			if(i >= 0)
			{
				return i;
			}
		}

		return -1;
		
	}

	public static void Reset()
	{
		if(digitalChannels.Keys.Contains(CurrentDevice)) digitalChannels[CurrentDevice].Clear();
		Interface.ClearAllDigital();
		Interface.ClearAllAnalog();
	}

	public static bool ReadDigitalChannel(DigitalChannel channel)
	{   
		if(!IsConnected) return false;
		if(channel < 0)
		{
			return (bool)(digitalChannels[CurrentDevice]?.Contains(channel));
		}
		return Interface.ReadDigitalChannel((int)channel);
	}

	public static void SetDigitalChannel(DigitalChannel Channel)
	{
		if(!IsConnected || Channel > 0) return;
		if (digitalChannels.ContainsKey(CurrentDevice))
		{
			if(!digitalChannels[CurrentDevice].Contains(Channel)) digitalChannels[CurrentDevice].Add(Channel);
		} else
		{
            digitalChannels.Add(CurrentDevice, [Channel]);
		}
		Interface.SetDigitalChannel(-(int)Channel);
	}

	public static void ClearDigitalChannel(DigitalChannel Channel)
	{
		if (!IsConnected || Channel > 0) return;
		if (digitalChannels.ContainsKey(CurrentDevice) && digitalChannels[CurrentDevice].Contains(Channel)) digitalChannels[CurrentDevice].Remove(Channel);
		Interface.ClearDigitalChannel(-(int)Channel);
	}

	public static void SetAnalogChannel(int Channel)
	{
		Interface.SetAnalogChannel(Channel);
	}

	public static int ReadAnalogChannel(int Channel)
	{
		return Interface.ReadAnalogChannel(Channel);
	}

    private static void UpdateConnectedDevice()
	{
		List<int> connectedDevices = new(ConnectedDevices);
        ConnectedDevices.Clear();
        foreach (int i in GetAvailableDevices())
		{
			if (connectedDevices.Contains(i)) ConnectedDevices.Add(i);
		}
	}

	private static void UpdateDigitalsChannel()
	{
        int digitalChannel = Interface.ReadAllDigital();
		if (!digitalChannels.ContainsKey(CurrentDevice)) return;
		digitalChannels[CurrentDevice].RemoveAll(new Predicate<DigitalChannel>((e) => { return e == DigitalChannel.B1 || e == DigitalChannel.B2 || e == DigitalChannel.B3 || e == DigitalChannel.B4 || e == DigitalChannel.B5; } ));
		if ((digitalChannel & 1) > 0) 
		{
			digitalChannels[CurrentDevice].Add(DigitalChannel.B1);
			OnDigitalChannelsChange?.Invoke(DigitalChannel.B1);

        }
        if ((digitalChannel & 2) > 0)
        {
            digitalChannels[CurrentDevice].Add(DigitalChannel.B2);
            OnDigitalChannelsChange?.Invoke(DigitalChannel.B2);
        }
        if ((digitalChannel & 3) > 0)
        {
            digitalChannels[CurrentDevice].Add(DigitalChannel.B3);
            OnDigitalChannelsChange?.Invoke(DigitalChannel.B3);
        }
        if ((digitalChannel & 4) > 0)
        {
            digitalChannels[CurrentDevice].Add(DigitalChannel.B4);
            OnDigitalChannelsChange?.Invoke(DigitalChannel.B4);
        }
        if ((digitalChannel & 5) > 0)
        {
            digitalChannels[CurrentDevice].Add(DigitalChannel.B5);
            OnDigitalChannelsChange?.Invoke(DigitalChannel.B5);
        }
    }
}

