/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Util.cs,v 1.4 2012/03/17 14:34:23 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//using Microsoft.JScript;
using System.CodeDom.Compiler;

#if UNITTEST
using System.Configuration;
using NUnit.Framework;
#endif

using Poderosa.Boot;

namespace Poderosa {
    /// <summary>
    /// <ja>
    /// 標準的な成功／失敗を示します。
    /// </ja>
    /// <en>
    /// A standard success/failure is shown. 
    /// </en>
    /// </summary>
    public enum GenericResult {
        /// <summary>
        /// <ja>成功しました</ja>
        /// <en>Succeeded</en>
        /// </summary>
        Succeeded,
        /// <summary>
        /// <ja>失敗しました</ja>
        /// <en>Failed</en>
        /// </summary>
        Failed
    }

    //Debug.WriteLineIfあたりで使用
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class DebugOpt {
#if DEBUG
        public static bool BuildToolBar = false;
        public static bool CommandPopup = false;
        public static bool DrawingPerformance = false;
        public static bool DumpDocumentRelation = false;
        public static bool IntelliSense = false;
        public static bool IntelliSenseMenu = false;
        public static bool LogViewer = false;
        public static bool Macro = false;
        public static bool MRU = false;
        public static bool PromptRecog = false;
        public static bool Socket = false;
        public static bool SSH = false;
        public static bool ViewManagement = false;
        public static bool WebBrowser = false;
#else //RELEASE
        public static bool BuildToolBar = false;
        public static bool CommandPopup = false;
        public static bool DrawingPerformance = false;
        public static bool DumpDocumentRelation = false;
        public static bool IntelliSense = false;
        public static bool IntelliSenseMenu = false;
        public static bool LogViewer = false;
        public static bool Macro = false;
        public static bool MRU = false;
        public static bool PromptRecog = false;
        public static bool Socket = false;
        public static bool SSH = false;
        public static bool ViewManagement = false;
        public static bool WebBrowser = false;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class RuntimeUtil {
        public static void ReportException(Exception ex) {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);

            string errorfile = ReportExceptionToFile(ex);

            //メッセージボックスで通知。
            //だがこの中で例外が発生することがSP1ではあるらしい。しかもそうなるとアプリが強制終了だ。
            //Win32のメッセージボックスを出しても同じ。ステータスバーなら大丈夫のようだ
            try {
                string msg = String.Format(InternalPoderosaWorld.Strings.GetString("Message.Util.InternalError"), errorfile, ex.Message);
                MessageBox.Show(msg, "Poderosa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception ex2) {
                Debug.WriteLine("(MessageBox.Show() failed) " + ex2.Message);
                Debug.WriteLine(ex2.StackTrace);
            }
        }
        public static void SilentReportException(Exception ex) {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            ReportExceptionToFile(ex);
        }
        public static void DebuggerReportException(Exception ex) {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }
        //ファイル名を返す
        private static string ReportExceptionToFile(Exception ex) {
            string errorfile = null;
            //エラーファイルに追記
            StreamWriter sw = null;
            try {
                sw = GetErrorLog(ref errorfile);
                ReportExceptionToStream(ex, sw);
            }
            finally {
                if (sw != null)
                    sw.Close();
            }
            return errorfile;
        }
        private static void ReportExceptionToStream(Exception ex, StreamWriter sw) {
            sw.WriteLine(DateTime.Now.ToString());
            sw.WriteLine(ex.Message);
            sw.WriteLine(ex.StackTrace);
            //inner exceptionを順次
            Exception i = ex.InnerException;
            while (i != null) {
                sw.WriteLine("[inner] " + i.Message);
                sw.WriteLine(i.StackTrace);
                i = i.InnerException;
            }
        }
        private static StreamWriter GetErrorLog(ref string errorfile) {
            errorfile = PoderosaStartupContext.Instance.ProfileHomeDirectory + "error.log";
            return new StreamWriter(errorfile, true/*append!*/, Encoding.Default);
        }

        public static Font CreateFont(string name, float size) {
            try {
                return new Font(name, size);
            }
            catch (ArithmeticException) {
                //JSPagerの件で対応。msvcr71がロードできない環境もあるかもしれないので例外をもらってはじめて呼ぶようにする
                Win32.ClearFPUOverflowFlag();
                return new Font(name, size);
            }
        }

        public static string ConcatStrArray(string[] values, char delimiter) {
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < values.Length; i++) {
                if (i > 0)
                    bld.Append(delimiter);
                bld.Append(values[i]);
            }
            return bld.ToString();
        }

        //min未満はmin, max以上はmax、それ以外はvalueを返す
        public static int AdjustIntRange(int value, int min, int max) {
            Debug.Assert(min <= max);
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class ParseUtil {
        public static bool ParseBool(string value, bool defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                return Boolean.Parse(value);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }
        public static byte ParseByte(string value, byte defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                return Byte.Parse(value);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }
        public static int ParseInt(string value, int defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                return Int32.Parse(value);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }
        public static float ParseFloat(string value, float defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                return Single.Parse(value);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }
        public static int ParseHexInt(string value, int defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
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
                    int v;
                    if (Int32.TryParse(t, System.Globalization.NumberStyles.HexNumber, null, out v))
                        return Color.FromArgb(v);
                }
                else if (t.Length == 6) {
                    int v;
                    if (Int32.TryParse(t, System.Globalization.NumberStyles.HexNumber, null, out v))
                        return Color.FromArgb((int)((uint)v | 0xFF000000)); //'A'要素は0xFFに
                }
                Color c = Color.FromName(t);
                return c.ToArgb() == 0 ? defaultvalue : c; //へんな名前だったとき、ARGBは全部0になるが、IsEmptyはfalse。なのでこれで判定するしかない
            }
        }

        public static T ParseEnum<T>(string value, T defaultvalue) {
            try {
                if (value == null || value.Length == 0)
                    return defaultvalue;
                else
                    return (T)Enum.Parse(typeof(T), value, false);
            }
            catch (Exception) {
                return defaultvalue;
            }
        }

        public static bool TryParseEnum<T>(string value, ref T parsed) {
            if (value == null || value.Length == 0) {
                return false;
            }

            try {
                parsed = (T)Enum.Parse(typeof(T), value, false);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        //TODO Generics化
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
    }


#if UNITTEST
    public static class UnitTestUtil {
        public static void Trace(string text) {
            Console.Out.WriteLine(text);
            Debug.WriteLine(text);
        }

        public static void Trace(string fmt, params object[] args) {
            Trace(String.Format(fmt, args));
        }

        //configuration fileからロード 現在はPoderosa.Monolithic.configファイルを仮定
        public static string GetUnitTestConfig(string entry_name) {
            string r = ConfigurationManager.AppSettings[entry_name];
            if (r == null)
                Assert.Fail("the entry \"{0}\" is not found in Poderosa.Monolithic.config file.", entry_name);
            return r;
        }

        public static string DumpStructuredText(StructuredText st) {
            StringWriter wr = new StringWriter();
            new TextStructuredTextWriter(wr).Write(st);
            wr.Close();
            return wr.ToString();
        }
    }

    [TestFixture]
    public class RuntimeUtilTests {
        [Test] //ごくふつうのケース
        public void ParseColor1() {
            Color c1 = Color.Red;
            Color c2 = ParseUtil.ParseColor("Red", Color.White);
            Assert.AreEqual(c1, c2);
        }
        [Test] //hex 8ケタのARGB
        public void ParseColor2() {
            Color c1 = Color.FromArgb(10, 20, 30);
            Color c2 = ParseUtil.ParseColor("FF0A141E", Color.White);
            Assert.AreEqual(c1, c2);
        }
        [Test] //hex 6ケタのRGB
        public void ParseColor3() {
            Color c1 = Color.FromArgb(10, 20, 30);
            Color c2 = ParseUtil.ParseColor("0A141E", Color.White);
            Assert.AreEqual(c1, c2);
        }
        [Test] //KnownColorでもOK
        public void ParseColor4() {
            Color c1 = Color.FromKnownColor(KnownColor.WindowText);
            Color c2 = ParseUtil.ParseColor("WindowText", Color.White);
            Assert.AreEqual(c1, c2);
        }
        [Test] //ARGBは一致でもColorの比較としては不一致
        public void ParseColor5() {
            Color c1 = Color.Blue;
            Color c2 = ParseUtil.ParseColor("0000FF", Color.White);
            Assert.AreNotEqual(c1, c2);
            Assert.AreEqual(c1.ToArgb(), c2.ToArgb());
        }
        [Test]　//知らない名前はラストの引数と一致
        public void ParseColor6() {
            Color c1 = Color.White;
            Color c2 = ParseUtil.ParseColor("asdfghj", Color.White); //パースできない場合
            Assert.AreEqual(c1, c2);
        }
        [Test] //ついでなので仕様確認 ToString()ではだめですよ
        public void ColorToString() {
            Color c1 = Color.Red;
            Color c2 = Color.FromName("Red");
            Color c3 = Color.FromKnownColor(KnownColor.WindowFrame);
            Color c4 = Color.FromArgb(255, 0, 0);

            Assert.AreEqual(c1, c2);
            Assert.AreEqual("Red", c1.Name);
            Assert.AreEqual("Red", c2.Name);
            Assert.AreEqual("WindowFrame", c3.Name);
            Assert.AreEqual("ffff0000", c4.Name);
        }
    }


#endif
}
