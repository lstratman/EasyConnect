/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Util.cs,v 1.3 2012/02/19 08:19:49 kzmi Exp $
 */
using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Threading;

using Poderosa.Preferences;

namespace Poderosa {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class GUtil {
        /*
        public static int ParseHexInt(string value, int defaultvalue) {
            try {
                return Int32.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }
        public static Color ParseColor(string t, Color defaultvalue) {
            if (t == null || t.Length == 0)
                return defaultvalue;
            else {
                if (t.Length == 8) { //16進で保存されていることもある。窮余の策でこのように
                    try {
                        int v = Int32.Parse(t, System.Globalization.NumberStyles.HexNumber);
                        return Color.FromArgb(v);
                    }
                    catch (Exception) {
                    }
                }
                return Color.FromName(t);
            }
        }
        public static ValueType ParseEnum(Type enumtype, string t, ValueType defaultvalue) {
            try {
                if (t == null || t.Length == 0)
                    return (ValueType)Enum.ToObject(enumtype, (int)defaultvalue);
                else
                    return (ValueType)Enum.Parse(enumtype, t, false);
            }
            catch (Exception) {
                return (ValueType)Enum.ToObject(enumtype, (int)defaultvalue);
            }
        }
        public static ValueType ParseMultipleEnum(Type enumtype, string t, ValueType defaultvalue) {
            try {
                int r = 0;
                foreach (string a in t.Split(','))
                    r |= (int)Enum.Parse(enumtype, a, false);
                return r;
            }
            catch (FormatException) {
                return defaultvalue;
            }
        }
        */



        public static string FileToDir(string filename) {
            int n = filename.LastIndexOf('\\');
            if (n == -1)
                throw new FormatException("filename does not include \\");

            return filename.Substring(0, n);
        }
        public static void Warning(IWin32Window owner, string msg, string caption) {
            MessageBox.Show(owner, msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void Warning(IWin32Window owner, string msg, MessageBoxIcon icon) {
            MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.OK, icon);
        }
        public static void Warning(IWin32Window owner, string msg) {
            MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static DialogResult AskUserYesNo(IWin32Window owner, string msg) {
            return MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        public static DialogResult AskUserYesNo(IWin32Window owner, string msg, MessageBoxIcon icon) {
            return MessageBox.Show(owner, msg, GEnv.Strings.GetString("Common.MessageBoxTitle"), MessageBoxButtons.YesNo, icon);
        }
        private static void InternalReportCriticalError(string remark, Exception ex) {
            Debug.WriteLine(remark);
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);

            //エラーファイルに追記
            string dir = null;
            StreamWriter sw = GetDebugLog(ref dir);
            sw.WriteLine(DateTime.Now.ToString() + remark + ex.Message);
            sw.WriteLine(ex.StackTrace);
            //inner exceptionを順次
            Exception i = ex.InnerException;
            while (i != null) {
                sw.WriteLine("[inner] " + i.Message);
                sw.WriteLine(i.StackTrace);
                i = i.InnerException;
            }
            sw.Close();

            //メッセージボックスで通知。
            //だがこの中で例外が発生することがSP1ではあるらしい。しかもそうなるとアプリが強制終了だ。
            //Win32のメッセージボックスを出しても同じ。ステータスバーなら大丈夫のようだ
            //...しかし、それでもNullReferenceExceptionあるいはExecutionEngineException(!)が発生する場合がある。Win32呼び出しでもだめだともう手出しできんな。あきらめてコメントアウト
            try {
                string msg = String.Format(GEnv.Strings.GetString("Message.GUtil.InternalError"), dir, ex.Message);
                MessageBox.Show(msg, "Poderosa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception ex2) {
                Debug.WriteLine(ex2.Message);
                Debug.WriteLine(ex2.StackTrace);
            }
        }
        private static StreamWriter GetDebugLog(ref string dir) {
            try {
                dir = AppDomain.CurrentDomain.BaseDirectory;
                return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
            }
            catch (Exception) {
                dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Poderosa";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
            }
        }

        private static StreamWriter _debugLog = null;
        public static void WriteDebugLog(string data) {
            string dir = null;
            if (_debugLog == null)
                _debugLog = GetDebugLog(ref dir);
            _debugLog.WriteLine(data);
            _debugLog.Flush();
        }



        //次に設定すべきログのファイル名 hostがnullだとそこは空白になる
        public static string CreateLogFileName(string host) {
            DateTime now = DateTime.Now;
            string date = String.Format("{0}{1,2:D2}{2,2:D2}", now.Year, now.Month, now.Day);

            string basefile;
            if (host == null || host.Length == 0)
                basefile = String.Format("{0}\\{1}", GEnv.Options.DefaultLogDirectory, date);
            else {
                if (host.StartsWith("rsp://"))
                    host = host.Substring(6); //rsp://のあとの文字列
                basefile = String.Format("{0}\\{1}_{2}", GEnv.Options.DefaultLogDirectory, ReplaceBadPathChar(host), date);
            }

            int n = 1;
            do {
                string filename;
                if (n == 1)
                    filename = String.Format("{0}.log", basefile);
                else
                    filename = String.Format("{0}_{1}.log", basefile, n);

                if (!File.Exists(filename))
                    return filename;
                else
                    n++;
            } while (true);
        }

        public static string ReplaceBadPathChar(string src) {
            char ch = '_';
            return src.Replace('\\', ch).Replace('/', ch).Replace(':', ch).Replace(';', ch).
                Replace(',', ch).Replace('*', ch).Replace('?', ch).Replace('"', ch).
                Replace('<', ch).Replace('>', ch).Replace('|', ch);
        }



        public static void WriteNameValue(TextWriter wr, string name, string value) {
            wr.Write(name);
            wr.Write('=');
            wr.WriteLine(value);
        }

        public static string[] EncodingDescription(Encoding[] src) {
            string[] t = new string[src.Length];
            for (int i = 0; i < src.Length; i++)
                t[i] = src[i].WebName;
            return t;
        }

        public static bool IsCursorKey(Keys key) {
            return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down;
        }


        //KeyStringの逆変換　KeyConverterの実装は死ぬほど遅い
        public static Keys ParseKey(string s) {
            if (s.Length == 0)
                return Keys.None;
            else if (s.Length == 1) {
                char ch = s[0];
                if ('0' <= ch && ch <= '9')
                    return Keys.D0 + (ch - '0');
                else
                    return (Keys)Enum.Parse(typeof(Keys), s);
            }
            else
                return (Keys)Enum.Parse(typeof(Keys), s);
        }
        public static Keys ParseKey(string[] value) { //modifier込みでパース
            Keys modifier = Keys.None;
            for (int i = 0; i < value.Length - 1; i++) { //最後以外
                string m = value[i];
                if (m == "Alt")
                    modifier |= Keys.Alt;
                else if (m == "Shift")
                    modifier |= Keys.Shift;
                else if (m == "Ctrl")
                    modifier |= Keys.Control;
                else
                    throw new Exception(m + " is unknown modifier");
            }
            return modifier | GUtil.ParseKey(value[value.Length - 1]);
        }

        //キーから対応するコントロールコード(ASCII 0 から 31まで)に変換する。対応するものがなければ-1
        public static int KeyToControlCode(Keys key) {
            Keys modifiers = key & Keys.Modifiers;
            Keys body = key & Keys.KeyCode;
            if (modifiers == Keys.Control) {
                int ib = (int)body;
                if ((int)Keys.A <= ib && ib <= (int)Keys.Z)
                    return ib - (int)Keys.A + 1;
                else if (body == Keys.Space)
                    return 0;
                else
                    return -1;
            }
            else
                return -1;
        }

        public static Thread CreateThread(ThreadStart st) {
            Thread t = new Thread(st);
            //t.ApartmentState = ApartmentState.STA;
            return t;
        }

    }

}
