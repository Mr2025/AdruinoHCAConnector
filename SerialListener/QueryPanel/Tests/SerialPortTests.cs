using System;
using System.Threading;
using NUnit.Framework;


using OpenNETCF.IO.Ports;

public class mySocket
{
	public static void Main(string[] args) 
	{
		SerialPortTests s = new SerialPortTests();
		try
		{
			s.Init();
			s.EnumeratePorts();
			s.OpenClose();
//			s.Test_BB_232SDA12();
		}
		finally
		{
			s.Dispose();
		}
	}  
}


/// <summary>Some simple Tests.</summary>
/// 
[TestFixture] 
public class SerialPortTests
{
	SerialPort port;
	
	[TestFixtureSetUp] public void Init() 
	{
		port = new SerialPort("com1");
	}

	[TestFixtureTearDown] public void Dispose() 
	{
		if( port != null )
			port.Dispose();
	}

	[Test] public void OpenClose() 
	{
		port.Open();
		Assert.IsTrue( port.IsOpen );
		port.Close();
		Thread.Sleep( 500 );
	}

	[Test] public void Test_BB_232SDA12() 
	{
		string	cmd		= "!0RA\0";
		byte	channels= 13;
		byte[]	bufout	= System.Text.Encoding.ASCII.GetBytes( cmd );
		byte[]	bufin	= new byte[channels * 2];

		bufout[ bufout.Length - 1 ] = (byte)(channels - 1);

		Assert.IsTrue( channels >= 1 && channels <= 13, "channels must be between 1 and 13" );

		port.Open();
		port.Write( bufout, 0, bufout.Length );
		Thread.Sleep( 1000 );	// wait 100 ms
		Assert.AreEqual( bufin.Length, port.Read( bufin, 0, bufin.Length ));
		port.Close();
	}

	[Test] public void EnumeratePorts() 
	{
		string[] ports = SerialPort.GetPortNames();
		Assert.IsTrue( ports.Length > 0, "no ports found" );

		Console.Write("Available ports:");
		foreach( string p in ports )
			Console.Write( " {0}", p );
		Console.WriteLine();
	}
}
