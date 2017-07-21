/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Logger.cs,v 1.5 2012/01/30 12:55:44 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;

using Poderosa.Document;
using Poderosa.Protocols;
using Poderosa.Util;
using Poderosa.ConnectionParam;

namespace Poderosa.Terminal {
    internal abstract class LoggerBase {
        private readonly ISimpleLogSettings _logSetting;
        private int _writeNumber = 0;
        private int _prevWriteNumber = 0;

        public abstract void Flush();

        public ISimpleLogSettings LogSettings {
            get {
                return _logSetting;
            }
        }

        public LoggerBase(ISimpleLogSettings log) {
            Debug.Assert(log != null);
            _logSetting = log;
        }

        public void AutoFlush() {
            int w = _writeNumber;
            if (w != _prevWriteNumber) {
                Flush();
                _prevWriteNumber = w;
            }
        }

        protected void Wrote() {
            _writeNumber++;
        }
    }


    internal class BinaryLogger : LoggerBase, IBinaryLogger {
        private readonly Stream _strm;
        private readonly object _sync = new object();
        private bool _closed = false;

        public BinaryLogger(ISimpleLogSettings log, Stream s)
            : base(log) {
            _strm = s;
        }

        public void Write(ByteDataFragment data) {
            lock (_sync) {
                if (!_closed) {
                    _strm.Write(data.Buffer, data.Offset, data.Length);
                    Wrote();
                }
            }
        }

        public override void Flush() {
            // note that Flush() may be called by AutoFlush()
            // even if output stream has been already closed.
            lock (_sync) {
                if (!_closed) {
                    _strm.Flush();
                }
            }
        }

        public void Close() {
            lock (_sync) {
                if (!_closed) {
                    _strm.Close();
                    _closed = true;
                }
            }
        }
    }

    internal class DefaultLogger : LoggerBase, ITextLogger {

        private readonly StreamWriter _writer;
        private readonly bool _withTimestamp;
        private readonly char[] _timestampBuffer;
        private readonly object _sync = new object();
        private bool _continued = false;
        private bool _closed = false;

        public DefaultLogger(ISimpleLogSettings log, StreamWriter w, bool withTimestamp)
            : base(log) {
            _writer = w;
            _withTimestamp = withTimestamp;
            if (withTimestamp)
                _timestampBuffer = new char[26];  // "YYYY-MM-DD hh:mm:ss,nnn - "
            else
                _timestampBuffer = null;
        }

        public void WriteLine(GLine line) {
            lock (_sync) {
                if (_closed)
                    return;

                if (_withTimestamp && !_continued)
                    WriteTimestamp();
                line.WriteTo(
                    delegate(char[] buff, int len) {
                        _writer.Write(buff, 0, len);
                    },
                    0);
                if (line.EOLType == EOLType.Continue) {
                    _continued = true;
                }
                else {
                    _continued = false;
                    _writer.WriteLine();
                }

                Wrote();
            }
        }

        public override void Flush() {
            // note that Flush() may be called by AutoFlush()
            // even if output stream has been already closed.
            lock (_sync) {
                if (!_closed) {
                    _writer.Flush();
                }
            }
        }
        
        public void Close() {
            lock (_sync) {
                if (!_closed) {
                    _writer.Close();
                    _closed = true;
                }
            }
        }

        public void Comment(string comment) {
            lock (_sync) {
                if (!_closed) {
                    _writer.Write(comment);
                    if (_withTimestamp && !_continued)
                        _writer.WriteLine();
                    Wrote();
                }
            }
        }

        private void WriteTimestamp() {
            // Write timestamp in ISO 8601 format.
            char[] buff = _timestampBuffer;
            int offset = 0;

            DateTime dt = DateTime.Now;

            offset = WriteInt(buff, offset, 4, dt.Year);
            buff[offset++] = '-';
            offset = WriteInt(buff, offset, 2, dt.Month);
            buff[offset++] = '-';
            offset = WriteInt(buff, offset, 2, dt.Day);
            buff[offset++] = 'T';
            offset = WriteInt(buff, offset, 2, dt.Hour);
            buff[offset++] = ':';
            offset = WriteInt(buff, offset, 2, dt.Minute);
            buff[offset++] = ':';
            offset = WriteInt(buff, offset, 2, dt.Second);
            buff[offset++] = '.';
            offset = WriteInt(buff, offset, 3, dt.Millisecond);

            // separator
            buff[offset++] = ' ';
            buff[offset++] = '-';
            buff[offset++] = ' ';

            _writer.Write(buff, 0, offset);
        }

        private static int WriteInt(char[] buff, int offset, int width, int value) {
            int limit = offset + width;
            int index = limit;
            for (int i = 0; i < width; i++) {
                buff[--index] = (char)('0' + value % 10);
                value /= 10;
            }
            return limit;
        }
    }

    //複数のログを取るための分岐
    internal class BinaryLoggerList : ListenerList<IBinaryLogger>, IBinaryLogger {
        public void Write(ByteDataFragment data) {
            if (this.IsEmpty)
                return;

            foreach (IBinaryLogger logger in this) {
                logger.Write(data);
            }
        }

        public void Close() {
            if (this.IsEmpty)
                return;

            foreach (IBinaryLogger logger in this) {
                logger.Close();
            }
            base.Clear();
        }

        public void Flush() {
            if (this.IsEmpty)
                return;

            foreach (IBinaryLogger logger in this) {
                logger.Flush();
            }
        }

        public void AutoFlush() {
            if (this.IsEmpty)
                return;

            foreach (IBinaryLogger logger in this) {
                logger.AutoFlush();
            }
        }
    }

    internal class TextLoggerList : ListenerList<ITextLogger>, ITextLogger {
        public void WriteLine(GLine line) {
            if (this.IsEmpty)
                return;
            foreach (ITextLogger logger in this) {
                logger.WriteLine(line);
            }
        }

        public void Comment(string comment) {
            if (this.IsEmpty)
                return;
            foreach (ITextLogger logger in this) {
                logger.Comment(comment);
            }
        }

        public void Close() {
            if (this.IsEmpty)
                return;
            foreach (ITextLogger logger in this) {
                logger.Close();
            }
            base.Clear();
        }

        public void Flush() {
            if (this.IsEmpty)
                return;
            foreach (ITextLogger logger in this) {
                logger.Flush();
            }
        }

        public void AutoFlush() {
            if (this.IsEmpty)
                return;
            foreach (ITextLogger logger in this) {
                logger.AutoFlush();
            }
        }
    }

    internal class XmlLoggerList : ListenerList<IXmlLogger>, IXmlLogger {
        
        public void Write(char ch) {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.Write(ch);
            }
        }

        public void EscapeSequence(char[] body) {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.EscapeSequence(body);
            }
        }

        public void Comment(string comment) {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.Comment(comment);
            }
        }

        public void Close() {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.Close();
            }
            base.Clear();
        }

        public void Flush() {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.Flush();
            }
        }

        public void AutoFlush() {
            if (this.IsEmpty)
                return;
            foreach (IXmlLogger logger in this) {
                logger.AutoFlush();
            }
        }
    }

    //ログに関する機能のまとめクラス
    internal class LogService : ILogService {
        private BinaryLoggerList _binaryLoggers;
        private TextLoggerList _textLoggers;
        private XmlLoggerList _xmlLoggers;

        private Thread _autoFlushThread = null;
        private readonly object _autoFlushSync = new object();

        private const int AUTOFLUSH_CHECK_INTERVAL = 1000;

        public LogService(ITerminalParameter param, ITerminalSettings settings) {
            _binaryLoggers = new BinaryLoggerList();
            _textLoggers = new TextLoggerList();
            _xmlLoggers = new XmlLoggerList();
            ITerminalEmulatorOptions opt = GEnv.Options;
            if (opt.DefaultLogType != LogType.None)
                ApplySimpleLogSetting(new SimpleLogSettings(opt.DefaultLogType, CreateAutoLogFileName(opt, param, settings)));
        }
        public void AddBinaryLogger(IBinaryLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while adding a new logger
                _binaryLoggers.Add(logger);
            }
            StartAutoFlushThread();
        }
        public void RemoveBinaryLogger(IBinaryLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while removing a logger
                _binaryLoggers.Remove(logger);
            }
        }
        public void AddTextLogger(ITextLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while adding a new logger
                _textLoggers.Add(logger);
            }
            StartAutoFlushThread();
        }
        public void RemoveTextLogger(ITextLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while removing a logger
                _textLoggers.Remove(logger);
            }
        }
        public void AddXmlLogger(IXmlLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while adding a new logger
                _xmlLoggers.Add(logger);
            }
            StartAutoFlushThread();
        }
        public void RemoveXmlLogger(IXmlLogger logger) {
            lock (_autoFlushSync) { // pause auto flush while removing a logger
                _xmlLoggers.Remove(logger);
            }
        }

        //以下はAbstractTerminalから
        public IBinaryLogger BinaryLogger {
            get {
                return _binaryLoggers;
            }
        }
        public ITextLogger TextLogger {
            get {
                return _textLoggers;
            }
        }
        public IXmlLogger XmlLogger {
            get {
                return _xmlLoggers;
            }
        }

        public void Flush() {
            _binaryLoggers.Flush();
            _textLoggers.Flush();
            _xmlLoggers.Flush();
        }
        public void Close(GLine last_line) {
            _textLoggers.WriteLine(last_line); //TextLogは改行ごとであるから、Close時に最終行を書き込むようにする
            StopAutoFlushThread();
            InternalClose();
        }
        private void InternalClose() {
            _binaryLoggers.Close();
            _textLoggers.Close();
            _xmlLoggers.Close();
        }
        public void Comment(string comment) {
            _textLoggers.Comment(comment);
            _xmlLoggers.Comment(comment);
        }

        public void ApplyLogSettings(ILogSettings settings, bool clear_previous) {
            if (clear_previous)
                InternalClose();
            ApplyLogSettingsInternal(settings);
        }
        private void ApplyLogSettingsInternal(ILogSettings settings) {
            ISimpleLogSettings sl = (ISimpleLogSettings)settings.GetAdapter(typeof(ISimpleLogSettings));
            if (sl != null) {
                ApplySimpleLogSetting(sl);
                return;
            }

            IMultiLogSettings ml = (IMultiLogSettings)settings.GetAdapter(typeof(IMultiLogSettings));
            if (ml != null) {
                foreach (ILogSettings e in ml)
                    ApplyLogSettingsInternal(e);
            }
        }
        private void ApplySimpleLogSetting(ISimpleLogSettings sl) {
            if (sl.LogType == LogType.None)
                return;

            FileStream fs = new FileStream(sl.LogPath, sl.LogAppend ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            ISimpleLogSettings loginfo = (ISimpleLogSettings)sl.Clone();
            switch (sl.LogType) {
                case LogType.Binary:
                    AddBinaryLogger(new BinaryLogger(loginfo, fs));
                    break;
                case LogType.Default:
                case LogType.PlainTextWithTimestamp:
                    bool withTimestamp = (sl.LogType == LogType.PlainTextWithTimestamp);
                    AddTextLogger(new DefaultLogger(loginfo, new StreamWriter(fs, Encoding.Default), withTimestamp));
                    break;
                case LogType.Xml:
                    AddXmlLogger(new XmlLogger(loginfo, new StreamWriter(fs, Encoding.Default)));
                    break;
            }
        }


        private static string CreateAutoLogFileName(ITerminalEmulatorOptions opt, ITerminalParameter param, ITerminalSettings settings) {
            IAutoLogFileFormatter[] fmts = TerminalEmulatorPlugin.Instance.AutoLogFileFormatter;
            string filebody;
            if (fmts.Length == 0) {
                DateTime now = DateTime.Now;
                filebody = String.Format("{0}\\{1}_{2}{3,2:D2}{4,2:D2}", opt.DefaultLogDirectory, ReplaceCharForLogFile(settings.Caption), now.Year, now.Month, now.Day);
            }
            else
                filebody = fmts[0].FormatFileName(opt.DefaultLogDirectory, param, settings);


            int n = 1;
            do {
                string filename;
                if (n == 1)
                    filename = String.Format("{0}.log", filebody);
                else
                    filename = String.Format("{0}_{1}.log", filebody, n);

                if (!File.Exists(filename))
                    return filename;
                else
                    n++;
            } while (true);
        }

        private static string ReplaceCharForLogFile(string src) {
            StringBuilder bld = new StringBuilder();
            foreach (char ch in src) {
                if (ch == '\\' || ch == '/' || ch == ':' || ch == ';' || ch == ',' || ch == '*' || ch == '?' || ch == '"' || ch == '<' || ch == '>' || ch == '|')
                    bld.Append('_');
                else
                    bld.Append(ch);
            }
            return bld.ToString();
        }

        private void StartAutoFlushThread() {
            if (_autoFlushThread != null)
                return;

            lock (_autoFlushSync) {
                if (_autoFlushThread == null) {
                    _autoFlushThread = new Thread(new ThreadStart(AutoFlushThread));
                    _autoFlushThread.Name = "LogService-AutoFlush";
                    _autoFlushThread.Start();
                }
            }
        }

        private void StopAutoFlushThread() {
            lock (_autoFlushSync) {
                Monitor.PulseAll(_autoFlushSync);
            }
            if (_autoFlushThread != null)
                _autoFlushThread.Join();
        }

        private void AutoFlushThread() {
            lock (_autoFlushSync) {
                while (true) {
                    _binaryLoggers.AutoFlush();
                    _textLoggers.AutoFlush();
                    _xmlLoggers.AutoFlush();

                    bool signaled = Monitor.Wait(_autoFlushSync, AUTOFLUSH_CHECK_INTERVAL);
                    if (signaled)
                        break;
                }
            }
        }
    }

    //基本マルチログ実装
    internal class MultiLogSettings : IMultiLogSettings {
        private List<ILogSettings> _data;

        public MultiLogSettings() {
            _data = new List<ILogSettings>();
        }

        public void Add(ILogSettings log) {
            Debug.Assert(log != null);
            _data.Add(log);
        }
        public void Remove(ILogSettings log) {
            if (_data.Contains(log)) {
                _data.Remove(log);
            }
        }
        public void Reset(ILogSettings log) {
            _data.Clear();
            _data.Add(log);
        }

        public ILogSettings Clone() {
            MultiLogSettings ml = new MultiLogSettings();
            foreach (ILogSettings l in _data)
                ml.Add(l.Clone());
            return ml;
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public IEnumerator<ILogSettings> GetEnumerator() {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _data.GetEnumerator();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum LogFileCheckResult {
        Create,
        Append,
        Cancel,
        Error
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public static class LogUtil {
        public static string SelectLogFileByDialog(Form parent) {
            using (SaveFileDialog dlg = new SaveFileDialog()) {
                dlg.AddExtension = true;
                dlg.DefaultExt = "log";
                dlg.Title = "Select Log";
                dlg.Filter = "Log Files(*.log)|*.log|All Files(*.*)|*.*";
                if (dlg.ShowDialog(parent) == DialogResult.OK) {
                    return dlg.FileName;
                }
                return null;
            }
        }

        //既存のファイルであったり、書き込み不可能だったら警告する
        public static LogFileCheckResult CheckLogFileName(string path, Form parent) {
            try {
                StringResource sr = GEnv.Strings;
                if (path.Length == 0) {
                    GUtil.Warning(parent, sr.GetString("Message.CheckLogFileName.EmptyPath"));
                    return LogFileCheckResult.Cancel;
                }

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) {
                    GUtil.Warning(parent, String.Format(sr.GetString("Message.CheckLogFileName.BadPathName"), path));
                    return LogFileCheckResult.Cancel;
                }

                if (File.Exists(path)) {
                    if ((FileAttributes.ReadOnly & File.GetAttributes(path)) != (FileAttributes)0) {
                        GUtil.Warning(parent, String.Format(sr.GetString("Message.CheckLogFileName.NotWritable"), path));
                        return LogFileCheckResult.Cancel;
                    }

                    Poderosa.Forms.ThreeButtonMessageBox mb = new Poderosa.Forms.ThreeButtonMessageBox();
                    mb.Message = String.Format(sr.GetString("Message.CheckLogFileName.AlreadyExist"), path);
                    mb.Text = sr.GetString("Util.CheckLogFileName.Caption");
                    mb.YesButtonText = sr.GetString("Util.CheckLogFileName.OverWrite");
                    mb.NoButtonText = sr.GetString("Util.CheckLogFileName.Append");
                    mb.CancelButtonText = sr.GetString("Util.CheckLogFileName.Cancel");
                    switch (mb.ShowDialog(parent)) {
                        case DialogResult.Cancel:
                            return LogFileCheckResult.Cancel;
                        case DialogResult.Yes: //上書き
                            return LogFileCheckResult.Create;
                        case DialogResult.No:  //追記
                            return LogFileCheckResult.Append;
                        default:
                            break;
                    }
                }

                return LogFileCheckResult.Create; //!!書き込み可能なディレクトリにあることを確認すればなおよし

            }
            catch (Exception ex) {
                GUtil.Warning(parent, ex.Message);
                return LogFileCheckResult.Error;
            }
        }

    }
}
