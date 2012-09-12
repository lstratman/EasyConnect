/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MacroInterface.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Collections;

using Poderosa.ConnectionParam;
#if !MACRODOC
using Poderosa.Config;
#endif

using Poderosa.Toolkit;

namespace Poderosa.Macro {

	/// <summary>
	/// <ja>マクロ機能のルートになるクラスです。</ja>
	/// <en>This class is the root of the macro functionality.</en>
	/// </summary>
	/// <remarks>
	/// <ja>マクロからこのクラスのインスタンスを作成して、各プロパティ・メソッドにアクセスしてください。</ja>
	/// <en>Use properties and methods after the macro creates an instance of this class. </en>
	/// </remarks>
	public sealed class Environment : MarshalByRefObject {

		/// <summary>
		/// <ja><see cref="ConnectionList"/>オブジェクトを取得します。</ja>
		/// <en>Gets the <see cref="ConnectionList"/> object.</en>
		/// </summary>
		public ConnectionList Connections {
			get {
				return _connectionList;
			}
		}

		/// <summary>
		/// <ja><see cref="Util"/>オブジェクトを取得します。</ja>
		/// <en>Gets the <see cref="Util"/> object.</en>
		/// </summary>
		public Util Util {
			get {
				return _util;
			}
		}

		/// <summary>
		/// <ja><see cref="Frame"/>オブジェクトを取得します。</ja>
		/// <en>Gets the <see cref="Frame"/> object.</en>
		/// </summary>
		public Frame Frame {
			get {
				return _frame;
			}
		}

		/// <summary>
		/// <ja>マクロのデバッグを補助するための<see cref="DebugService"/>オブジェクトを取得します。</ja>
		/// <en>Gets the <see cref="DebugService"/> object for debugging the macro.</en>
		/// </summary>
		public DebugService Debug {
			get {
				return _debugService;
			}
		}

		/// <summary>
		/// <ja>Poderosaのインストールされたディレクトリ名を取得します。末尾には \ がついています。</ja>
		/// <en>Gets the directory the Poderosa is installed. The tail of the string is a '\'.</en>
		/// </summary>
		public string InstalledDir {
			get {
				return _guevaraDir;
			}
		}

		/// <summary>
		/// <ja>Poderosa環境変数を取得します。</ja>
		/// <en>Gets the Poderosa environment variable.</en>
		/// </summary>
		/// <remarks>
		/// <ja>
		/// <para>　Poderosa環境変数は、ユーザの環境に依存する部分をマクロから分離するために用意された機能です。
		/// 環境変数の定義は、メニューからツール - マクロ - 環境設定を選び、その中で環境変数ボタンによって確認と編集ができます。</para>
		/// <para>　たとえば、テキストエディタのパスをここに登録させておいて、マクロから起動することができます。</para>
		/// <para>　なお、Poderosa環境変数はOSの環境変数とは関係ありません。</para>
		/// </ja>
		/// <en>
		/// <para> The Poderosa environment variable feature intends to separate configurations which depend on the environment of users from the macro.</para>
		/// <para> The user can edit Poderosa envrionment variables from the Tools - Macro - Configure Environment menu. For example, the user registers the text editor as an environment variable and the macro launchs the editor. </para>
		/// <para> Note that the environment variable has no relation to the environment variables of Windows. </para>
		/// </en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var filename = "C:\temp\...
		/// env.Util.Exec(env.GetVariable("tools.texteditor") + " " + filename);
		/// </code>
		/// </example>
		/// <param name="key"><ja>環境変数の名前</ja><en>the name of varialbe</en></param>
		/// <returns><ja>環境変数が定義されていればその値、定義されていなければnull</ja><en>If the variable is defined, returns the value. Otherwise, returns null.</en></returns>
		public string GetVariable(string key) {
#if MACRODOC
			return null;
#else
			return GApp.MacroManager.GetVariable(key, null);
#endif
		}
		/// <summary>
		/// <ja>Poderosa環境変数を取得します。</ja>
		/// <en>Gets a Poderosa environment variable.</en>
		/// </summary>
		/// <param name="key"><ja>環境変数の名前</ja><en>the name of varialbe</en></param>
		/// <param name="def"><ja>見つからなかったときに返すデフォルト値</ja><en>the default value in case that the key is not found</en></param>
		/// <returns><ja>環境変数が定義されていればその値、定義されていなければdefの値</ja><en>If the variable is defined, returns the value. Otherwise, returns the value of def.</en></returns>
		public string GetVariable(string key, string def) {
#if MACRODOC
			return null;
#else
			return GApp.MacroManager.GetVariable(key, def);
#endif
		}

		private static Version _version;
		private static ConnectionList _connectionList;
		private static Util _util;
		private static DebugService _debugService; 
		private static string _guevaraDir;
		private static Frame _frame;

		internal static void Init(ConnectionList cl, Util ui, Frame fr, DebugService ds) {
			_version = new Version(1,0);
			_connectionList = cl;
			_util = ui;
			_frame = fr;
			_debugService = ds;
			_guevaraDir = AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	/// <summary>
	/// <ja><see cref="Connection"/>オブジェクトのコレクションです。</ja>
	/// <en>A collection of <see cref="Connection"/> objects.</en>
	/// </summary>
	public abstract class ConnectionList : MarshalByRefObject, IEnumerable {
		/// <summary>
		/// <ja>コネクションの数です。</ja>
		/// <en>Gets the number of connections.</en>
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// <ja><see cref="Connection"/>オブジェクトを列挙します。</ja>
		/// <en>Enumerates each <see cref="Connection"/> objects.</en>
		/// </summary>
		public abstract IEnumerator GetEnumerator();

		/// <summary>
		/// <ja>アプリケーションでアクティブになっている接続を返します。</ja>
		/// <en>Returns the active connection of Poderosa.</en>
		/// <ja>アクティブな接続がないときはnullを返します。</ja>
		/// <en>If there are no active connections, returns null.</en>
		/// </summary>
		public abstract Connection ActiveConnection { get; }

		/// <summary>
		/// <ja>新しい接続を開きます。</ja>
		/// <en>Opens a new connection.</en>
		/// </summary>
		/// <remarks>
		/// <ja>失敗したときはメッセージボックスで通知をした上でnullが返ります。</ja>
		/// <en>If the connection fails, Poderosa shows an error message box and returns null to the macro.</en>
		/// </remarks>
		/// <seealso cref="TerminalParam"/>
		/// <seealso cref="TCPTerminalParam"/>
		/// <seealso cref="TelnetTerminalParam"/>
		/// <seealso cref="SSHTerminalParam"/>
		/// <seealso cref="SerialTerminalParam"/>
		/// <param name="param"><ja>接続に必要なパラメータを収録した<see cref="TerminalParam"/>オブジェクト</ja><en>The <see cref="TerminalParam"/> object that contains parameters for the connection.</en></param>
		/// <returns><ja>新しく開かれた<see cref="Connection"/>オブジェクト</ja><en>A <see cref="Connection"/> object that describes the new connection.</en></returns>
		public abstract Connection Open(TerminalParam param);

		/// <summary>
		/// <ja>ショートカットファイルを開きます</ja>
		/// <en>Opens a shortcut file</en>
		/// </summary>
		/// <remarks>
		/// <ja>接続が失敗したり、ユーザがキャンセルするとnullが返ります。</ja>
		/// <en>If the connection is failed or the user cancelled, this method returns null.</en>
		/// </remarks>
		/// <param name="filename"><ja>接続に必要なパラメータを収録したショートカットファイル名</ja><en>A shortcut file that contains parameters for the connection.</en></param>
		/// <returns><ja>新しく開かれた<see cref="Connection"/>オブジェクト</ja><en>A <see cref="Connection"/> object that describes the new connection.</en></returns>
		public abstract Connection OpenShortcutFile(string filename);

	}
	

	/// <summary>
	/// <ja>１本の接続を表します。</ja>
	/// <en>Describes a connection.</en>
	/// </summary>
	public abstract class Connection  : MarshalByRefObject {
		/// <summary>
		/// <ja>この接続に設定された画面の幅を文字単位で取得します。</ja>
		/// <en>Gets the width of the console in characters.</en>
		/// </summary>
		public abstract int TerminalWidth { get; }
		/// <summary>
		/// <ja>この接続に設定された画面の高さを文字単位で取得します。</ja>
		/// <en>Gets the height of the console in characters.</en>
		/// </summary>
		public abstract int TerminalHeight { get; }

		/// <summary>
		/// <ja>この接続をアクティブにし、最前面に持っていきます。</ja>
		/// <en>Activates this connection and brings to the front of application.</en>
		/// </summary>
		public abstract void Activate();

#if !MACRODOC
		/// <summary>
		/// この接続をアクティブにし、最前面に持っていきます。引数でペインの位置を指定できます。
		/// </summary>
		/// <param name="pos">ペインの位置を指定します。この引数はフレームが上下または左右に分割されているときのみ意味を持ちます。</param>
		public abstract void Activate(PanePosition pos);
#endif
		/// <summary>
		/// <ja>接続を閉じます。</ja>
		/// <en>Closes this connection.</en>
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// <ja>データを送信します。</ja>
		/// <en>Transmits data.</en>
		/// <ja>文字列はこの接続に設定されたエンコーディングに従ってバイト列にエンコードされます。</ja>
		/// <en>The string is encoded in accord with the encoding of this connection.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// connection.Transmit("ls");
		/// connection.TransmitLn("-la");
		/// </code>
		/// </example>
		/// <param name="data">送信したい文字列</param>
		public abstract void Transmit(string data);

		/// <summary>
		/// <ja>データにつづけて改行を送信します。</ja>
		/// <en>Transmits data followed by new line character.</en>
		/// </summary>
		/// <remarks>
		/// <ja>文字列はこの接続に設定されたエンコーディングに従ってバイト列にエンコードされます。</ja>
		/// <en>The string is encoded in accord with the encoding of this connection.</en>
		/// <ja>このメソッドは文字列の入力につづいてEnterキーを押すのと同じ効果があります。</ja>
		/// <en>This method has the same effect as pressing the Enter key following the input of the string.</en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// 
		/// var connection = Environment.Connections.ActiveConnection;
		/// connection.Transmit("ls");
		/// connection.TransmitLn("-la");
		/// </code>
		/// </example>
		/// <param name="data"><ja>入力データ</ja><en>The input data</en></param>
		public abstract void TransmitLn(string data);

		/// <summary>
		/// <ja>この接続に対してBreak信号を送ります。</ja>
		/// <en>Send a break signal to this connection.</en>
		/// </summary>
		/// <remarks>
		/// <ja>SSH1ではBreak信号を送ることはできません。</ja>
		/// <en>SSH1 does not support the break signal.</en>
		/// </remarks>
		public abstract void SendBreak();

		/// <summary>
		/// <ja>１行のデータを受信します。</ja>
		/// <en>Receives a line from the connection.</en>
		/// </summary>
		/// <remarks>
		/// <para><ja>　ホストからデータが未到着だったり行が終わっていないときは、１行の終了が確認できるまでメソッドはブロックします。</ja>
		/// <en> When no data is available or the new line characters are not sent, the execution of this method is blocked.</en></para>
		/// <para><ja>　特にプロンプト文字列は改行を含まないので、プロンプトを待つためにこのメソッドを使わないようにしてください。プロンプトの判定をするような場合にはかわりに<see cref="ReceiveData"/>メソッドを使ってください。</ja>
		/// <en> Especially note that this method could not be used to wait a prompt string since it does not contain new line characters. To wait a prompt, use the <see cref="ReceiveData"/> method instead of ReceiveLine method.</en>
		/// </para>
		/// <para><ja>　また、ホストから来るデータのうち、CRとNULは無視されます。</ja>
		/// <en> Additionally, CR and NUL are ignored in the data from the host.</en></para>
		/// <seealso cref="ReceiveData"/>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// var output = new StreamWriter("...
		/// var connection = env.Connections.ActiveConnection;
		/// var line = connection.ReceiveLine();
		/// while(line!="end") { //wait for "end"
		///   output.WriteLine(line);
		///   line = connection.ReceiveLine();
		/// }
		/// output.Close();
		/// 
		/// </code>
		/// </example>
		/// <returns><ja>受信した文字列です。改行文字は含みません。</ja><en>The received line without new line characters.</en></returns>
		public abstract string ReceiveLine();

		/// <summary>
		/// <ja>データを受信します。</ja>
		/// <en>Receives data from the connection.</en>
		/// </summary>
		/// <remarks>
		/// <para><ja>　ホストからデータが未到着のときは、到着するまでマクロの実行はブロックします。</ja>
		/// <en>  When no data is available, the execution of this method is blocked.</en>
		/// </para>
		/// <para><ja>　データが到着済みのときは、前回のReceiveDataの呼び出し以降に来たデータを一括して取得します。行に切り分ける作業はマクロ側で行う必要がありますが、改行で終わっていないデータも取得できる利点があります。 <see cref="ReceiveLine"/>と使い分けてください。</ja>
		/// <en> This method returns the whole data from the previous call of the ReceiveData method. Though this method can obtain the data even if it does not contain new line characters, the split into lines is responsible for the macro. Please compare to the <see cref="ReceiveLine"/> method.</en>
		/// </para>
		/// <para><ja>　また、ホストから来るデータのうち、CRとNULは無視されます。改行はLFによって判別します。</ja>
		/// <en> CR and NUL are ignored in the data from the host. The line breaks are determined by LF.</en></para>
		/// <seealso cref="ReceiveLine"/>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// var data = connection.ReceiveData();
		/// if(data.EndsWith("login: ") {
		///	  ...
		/// </code>
		/// </example>
		/// <returns><ja>受信した文字列です。</ja><en>The received data.</en></returns>
		public abstract string ReceiveData();

		/// <summary>
		/// <ja>ログにコメントを書きます。接続がログを取るように設定されていない場合は何もしません。</ja>
		/// <en>Writes a comment to the log. If the connection is not set to record the log, this method does nothing.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var connection = env.Connections.ActiveConnection;
		/// connection.WriteComment("starting macro...");
		/// </code>
		/// </example>
		/// <param name="comment"><ja>コメント文字列</ja><en>The comment string</en></param>
		public abstract void WriteComment(string comment);
	}

	/// <summary>
	/// <ja>アプリケーションの外観を制御するためのオブジェクトです。</ja>
	/// <en>Implements the appearances of the application.</en>
	/// </summary>
	public abstract class Frame : MarshalByRefObject {
#if !MACRODOC
		/// <summary>
		/// <ja>分割方式を取得・設定します。</ja>
		/// <en>Gets or sets the division style.</en>
		/// </summary>
		public abstract GFrameStyle FrameStyle { get; set; }
#endif
		/// <summary>
		/// <ja>フレームをシングルスタイルにセットするとともに、端末をリサイズします。</ja>
		/// <en>Changes the frame style to be single and resizes the terminal.</en>
		/// </summary>
		/// <param name="width"><ja>ターミナルの幅を文字単位で指定します。</ja><en>The terminal width in characters</en></param>
		/// <param name="height"><ja>ターミナルの高さを文字単位で指定します。</ja><en>The terminal height in characters</en></param>
		public abstract void SetStyleS(int width, int height);

		/// <summary>
		/// <ja>フレームを上下２分割スタイルにセットするとともに、端末をリサイズします。</ja>
		/// <en>Changes the frame style to be divided horizontally and resizes the terminal.</en>
		/// </summary>
		/// <param name="width"><ja>ターミナルの幅を文字単位で指定します。これは上下２つのペインで共有されます。</ja><en>The terminal width in characters. This is common to the two panes.</en></param>
		/// <param name="height1"><ja>上のペインの高さを文字単位で指定します。</ja><en>The terminal height of the upper pane in characters.</en></param>
		/// <param name="height2"><ja>下のペインの高さを文字単位で指定します。</ja><en>The terminal height of the lower pane in characters.</en></param>
		public abstract void SetStyleH(int width, int height1, int height2);

		/// <summary>
		/// <ja>フレームを左右２分割スタイルにセットするとともに、フレームをリサイズします。</ja>
		/// <en>Changes the frame style to be divided vertically and resizes the terminal.</en>
		/// </summary>
		/// <param name="width1"><ja>左のペインの幅を文字単位で指定します。</ja><en>The terminal width of the left pane in characters.</en></param>
		/// <param name="width2"><ja>右のペインの幅を文字単位で指定します。</ja><en>The terminal width of the right pane in characters.</en></param>
		/// <param name="height"><ja>ターミナルの高さを文字単位で指定します。これは左右２つのペインで共有されます。</ja><en>The terminal height in characters. This is common to the two panes.</en></param>
		public abstract void SetStyleV(int width1, int width2, int height);
	}


	/// <summary>
	/// <ja>マクロから呼び出すための、比較的よく使いそうな機能を収録したオブジェクトです。</ja>
	/// <en>Implements several utility functions for macros.</en>
	/// </summary>
	public abstract class Util : MarshalByRefObject {
		/// <summary>
		/// <ja>メッセージボックスを表示します。</ja>
		/// <en>Shows a message box.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// env.Util.MessageBox(String.Format("This file is {0}", env.MacroFileName));
		/// </code>
		/// </example>
		/// <param name="msg"><ja>表示したいメッセージ</ja><en>The message to be shown.</en></param>
		public abstract void MessageBox(string msg);

		/// <summary>
		/// <ja>ファイルを開く、印刷するといった操作をします。</ja>
		/// <en>Performs actions to the file such as open or print.</en>
		/// </summary>
		/// <remarks>
		/// <ja>
		/// 立ち上がるアプリケーションはファイルの拡張子とverb引数によって決まります。
		/// たとえば拡張子がtxtであればテキストエディタが起動します。
		/// 内部的には、このメソッドはWin32のShellExecute APIを呼び出します。
		/// </ja>
		/// <en>
		/// The application is decided by the extension and the verb argument.
		/// For exapmle, a text editor starts if the extension is .txt.
		/// This method calls the ShellExecute API of Win32 internally.
		/// </en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// import System.IO;
		/// var env = new Environment();
		/// 
		/// string filename = Path.GetTempFileName() + ".txt";
		/// ... (write some text to this file)
		/// 
		/// env.Util.ShellExecute("open", filename);
		/// </code>
		/// </example>
		/// <param name="verb"><ja>ファイルに対して行う動作です。"open","print"などです。</ja><en>The action to the file such as "open" or "print".</en></param>
		/// <param name="filename"><ja>開くファイルをフルパスで指定します。</ja><en>The full path of the file name.</en></param>
		public abstract void ShellExecute(string verb, string filename);

		/// <summary>
		/// <ja>任意のアプリケーションを起動します。</ja>
		/// <en>Starts other applications.</en>
		/// </summary>
		/// <remarks>
		/// <ja>内部的には、このメソッドはWin32のWinExec APIを呼び出します。</ja>
		/// <en>This method calls WinExec API of Win32 internally.</en>
		/// </remarks>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// env.Util.Exec("notepad.exe");
		/// </code>
		/// </example>
		/// <param name="command"><ja>起動したいアプリケーションの名前です。必要であれば引数をつけることもできます。</ja><en>The name of the application to be started. Arguments are allowed if necessary.</en></param>
		public abstract void Exec(string command);
	}

	/// <summary>
	/// <ja>マクロのテストとデバッグに必要な機能を提供します。</ja>
	/// <en>Implements features for testing and debugging the macro.</en>
	/// </summary>
	/// <remarks>
	/// <para><ja>　マクロのプロパティ画面において、「トレースウィンドウを表示する」オプションをつけておくと、そのマクロを起動するときにトレースウィンドウが使えるようになります。</ja>
	/// <en> The macro trace window is displayed when the "shows trace window" option is checked in the dialog box of the macro property.</en>
	/// </para>
	/// </remarks>
	public abstract class DebugService : MarshalByRefObject {
		/// <summary>
		/// <ja>トレースウィンドウに１行のデータを表示します。</ja>
		/// <en>Outputs a line to the trace window.</en>
		/// </summary>
		/// <example>
		/// <code>
		/// import Poderosa.Macro;
		/// var env = new Environment();
		/// 
		/// var i = 123;
		/// env.Debug.Trace(String.Format("i={0}", i));
		/// </code>
		/// </example>
		/// <param name="msg"><ja>表示したいデータ</ja><en>The data to be displayed.</en></param>
		public abstract void Trace(string msg);

		/// <summary>
		/// <ja>呼び出した時点でのスタックトレースを表示します。</ja>
		/// <en>Outputs the stack trace to the trace window.</en>
		/// </summary>
		public abstract void PrintStackTrace();
	}

}
