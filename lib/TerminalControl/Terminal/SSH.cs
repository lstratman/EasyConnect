/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.

 $Id: SSH.cs,v 1.2 2005/04/20 09:06:04 okajima Exp $
*/
using System;
using System.Text;
using System.Windows.Forms;

using Poderosa.ConnectionParam;

using Granados.SSHC;
using Granados.PKI;


namespace Poderosa.SSH
{
	//Granadosを使うやつはこちら　起動時にはロードしないようにするため
	public class LocalSSHUtil {
		public static CipherAlgorithm ParseCipherAlgorithm(string t) {
			if(t=="AES128")
				return CipherAlgorithm.AES128;
			else if(t=="Blowfish")
				return CipherAlgorithm.Blowfish;
			else if(t=="TripleDES")
				return CipherAlgorithm.TripleDES;
			else
				throw new Exception("Unknown CipherAlgorithm " + t);
		}
		public static CipherAlgorithm[] ParseCipherAlgorithm(string[] t) {
			CipherAlgorithm[] ret = new CipherAlgorithm[t.Length];
			int i = 0;
			foreach(string a in t) {
				ret[i++] = ParseCipherAlgorithm(a);
			}
			return ret;
		}
		public static string[] FormatPublicKeyAlgorithmList(PublicKeyAlgorithm[] value) {
			string[] ret = new string[value.Length];
			int i=0;
			foreach(PublicKeyAlgorithm a in value)
				ret[i++] = a.ToString();
			return ret;
		}

		public static CipherAlgorithm[] ParseCipherAlgorithmList(string value) {
			return ParseCipherAlgorithm(value.Split(','));
		}


		public static PublicKeyAlgorithm ParsePublicKeyAlgorithm(string t) {
			if(t=="DSA")
				return PublicKeyAlgorithm.DSA;
			else if(t=="RSA")
				return PublicKeyAlgorithm.RSA;
			else
				throw new Exception("Unknown CipherAlgorithm " + t);
		}
		public static PublicKeyAlgorithm[] ParsePublicKeyAlgorithm(string[] t) {
			PublicKeyAlgorithm[] ret = new PublicKeyAlgorithm[t.Length];
			int i = 0;
			foreach(string a in t) {
				ret[i++] = ParsePublicKeyAlgorithm(a);
			}
			return ret;
		}
		public static PublicKeyAlgorithm[] ParsePublicKeyAlgorithmList(string value) {
			return ParsePublicKeyAlgorithm(value.Split(','));
		}

		public static string SimpleEncrypt(string plain) {
			byte[] t = Encoding.ASCII.GetBytes(plain);
			if((t.Length % 16)!=0) {
				byte[] t2 = new byte[t.Length + (16 - (t.Length % 16))];
				Array.Copy(t, 0, t2, 0, t.Length);
				for(int i=t.Length+1; i<t2.Length; i++) //残りはダミー
					t2[i] = t[i % t.Length];
				t = t2;
			}

			byte[] key = Encoding.ASCII.GetBytes("- BOBO VIERI 32-");
			Granados.Crypto.Rijndael rijndael = new Granados.Crypto.Rijndael();
			rijndael.InitializeKey(key);

			byte[] e = new byte[t.Length];
			rijndael.encryptCBC(t, 0, t.Length, e, 0);

			return Encoding.ASCII.GetString(Granados.Toolkit.Base64.Encode(e));
		}
		public static string SimpleDecrypt(string enc) {
			byte[] t = Granados.Toolkit.Base64.Decode(Encoding.ASCII.GetBytes(enc));
			byte[] key = Encoding.ASCII.GetBytes("- BOBO VIERI 32-");
			Granados.Crypto.Rijndael rijndael = new Granados.Crypto.Rijndael();
			rijndael.InitializeKey(key);

			byte[] d = new byte[t.Length];
			rijndael.decryptCBC(t, 0, t.Length, d, 0);

			return Encoding.ASCII.GetString(d); //パディングがあってもNULL文字になるので除去されるはず
		}
	}

	//鍵のチェック関係
	public enum KeyCheckResult {
		OK,
		Different,
		NotExists
	}
	public interface ISSHKnownHosts {
		KeyCheckResult Check(SSHTerminalParam param, string server_key);
		void Update(SSHTerminalParam param, string server_key);
	}


	public class DefaultSSHKnownHosts : ISSHKnownHosts {
		public virtual KeyCheckResult Check(SSHTerminalParam param, string server_key) {
			return KeyCheckResult.OK;
		}
		public virtual void Update(SSHTerminalParam param, string server_key) {
		}
	}

	public class HostKeyChecker {
		private IWin32Window _parentForm;
		private SSHTerminalParam _tryingParam;
		public HostKeyChecker(IWin32Window parent, SSHTerminalParam param) {
			_parentForm = parent;
			_tryingParam = param;
		}
		public bool CheckHostKeyCallback(SSHConnectionInfo ci) {
            /*
			string keystr = ci.DumpHostKeyInKnownHostsStyle();
			KeyCheckResult r = GEnv.SSHKnownHosts.Check(_tryingParam, keystr);
			if(r==KeyCheckResult.NotExists) {
				if(GEnv.InterThreadUIService.AskUserYesNo(GEnv.Strings.GetString("Message.HostKeyChecker.AskHostKeyRegister"))==DialogResult.Yes) {
					GEnv.SSHKnownHosts.Update(_tryingParam, keystr);
					return true;
				}
				else
					return false;
			}
			else if(r==KeyCheckResult.Different) {
				if(GEnv.InterThreadUIService.AskUserYesNo(GEnv.Strings.GetString("Message.HostKeyChecker.AskHostKeyRenew"))==DialogResult.Yes) {
					GEnv.SSHKnownHosts.Update(_tryingParam, keystr);
					return true;
				}
				else
					return false;
			}
			else
				return true;
            */
            return true;
		}

	}
}
