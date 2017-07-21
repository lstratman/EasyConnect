/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

using Poderosa.Preferences;

namespace Poderosa.Commands {
    /// <summary>
    /// <ja>
    /// コマンドの実行結果を示します。
    /// </ja>
    /// <en>
    /// Return the result of the command.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の<see cref="IPoderosaCommand.InternalExecute">InternalExecuteメソッド</see>の実装者は、
    /// コマンドの実行の可否を、この列挙体で返します。
    /// </para>
    /// <para>
    /// 成功した場合にはSucceeded、失敗した場合にはFailedを返すように実装してください。
    /// </para>
    /// <para>
    /// Cancelledはユーザー操作によってキャンセルされた場合などに用います。
    /// </para>
    /// <para>
    /// Ignoredはコマンドを実行する対象がなかったとき（たとえば選択されたテキストに対して処理すべきコマンドの場合に、現在選択しているテキストがなかったときなど）に用います。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// Those who implement about the <see cref="IPoderosaCommand.InternalExecute">InternalExecute method</see> of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> return right or wrong of the execution of the command with this enumeration. 
    /// </para>
    /// <para>
    /// Please implement to return Failed when Succeeded and failing when succeeding. 
    /// </para>
    /// <para>
    /// Cancelled is used when canceled by the user operation. 
    /// </para>
    /// <para>
    /// Ignored is used when there is no object the execution of the command (For instance, there is no text that has been selected now for the command that should be processed to the selected text). 
    /// </para>
    /// </en>
    /// </remarks>
    public enum CommandResult {
        /// <summary>
        /// <ja>
        /// 成功
        /// </ja>
        /// <en>
        /// Succeeded.
        /// </en>
        /// </summary>
        Succeeded,
        /// <summary>
        /// <ja>
        /// 失敗
        /// </ja>
        /// <en>
        /// Failed
        /// </en>
        /// </summary>
        Failed,
        /// <summary>
        /// <ja>
        /// キャンセルした
        /// </ja>
        /// <en>
        /// Canceleld
        /// </en>
        /// </summary>
        Cancelled,
        /// <summary>
        /// <ja>
        /// 無視した
        /// </ja>
        /// <en>
        /// Ignored.
        /// </en>
        /// </summary>
        Ignored
    }

    //コマンドの駆動対象。コンテキストメニューを出す側が提供して、ICommand#Executeの引数になる。
    //メインメニュー配下の場合、メインウィンドウをIAdaptable経由で取得することになる。
    /// <summary>
    /// <ja>
    /// コマンドが実行すべきターゲットを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows target that command should execute.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// このインターフェイスは、<seealso cref="ICommandManager">ICommandManager</seealso>の<see cref="ICommandManager.Execute">Executeメソッド</see>を呼び出して、コマンドを実行する際に引き渡す
    /// ターゲットとして使われます。
    /// </para>
    /// <para>
    /// 渡されたターゲットは、<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の<see cref="IPoderosaCommand.InternalExecute">InternamExecuteメソッド</see>
    /// にそのまま引き渡されます。
    /// </para>
    /// <para>
    /// メニューやツールバーから呼び出されるコマンドは、<paramref name="target"/>には、メインウィンドウを示す<see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>
    /// が渡されることを想定しています。この種以外のインターフェイスが渡されたときには、
    /// 正しく動作しません。
    /// </para>
    /// <para>
    /// <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>を使うと、ターゲットをウィンドウやビューへと変換できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface is used as a target handed over when the <see cref="ICommandManager.Execute">Execute method</see> of <seealso cref="ICommandManager">ICommandManager</seealso> is called, and the command is executed. 
    /// 
    /// </para>
    /// <para>
    /// As for the passed target, off is passed to the <see cref="IPoderosaCommand.InternalExecute">InternamExecute method</see> of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> as it is. 
    /// </para>
    /// <para>
    /// The command called from the menu and the toolbar assumes to <paramref name="target"/> 
    /// <see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>'s that shows the main window being passed. 
    /// It doesn't operate correctly when you pass interfaces other than this kind.
    /// </para>
    /// <para>
    /// The target can be converted into the window and the view by using <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface ICommandTarget : IAdaptable {
    }

    //Menu/Commandの実行可否のdelegate
    /// <summary>
    /// <ja>メニューにチェックが付いているかどうかを決めるときに呼び出されるデリゲートです。</ja>
    /// <en>Delegate called when it is decided whether the check has adhered to the menu. </en>
    /// </summary>
    /// <param name="target">
    /// <ja>
    /// コマンドの対象を示すターゲットです。
    /// </ja>
    /// <en>
    /// Target that shows object of command.
    /// </en>
    /// </param>
    /// <returns>
    /// <ja>
    /// チェックが付いているならtrueを、そうでないならfalseを返してください。
    /// </ja>
    /// <en>
    /// Please return true and return false if it is not so if the check has adhered. 
    /// </en>
    /// </returns>
    /// <remarks>
    /// <ja>
    /// <para>
    /// <paramref name="target">target</paramref>にはアクティブウィンドウを示す<see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>（メインメニューの場合）またはアクティブビューを示す<see cref="Poderosa.Sessions.IPoderosaView">IPoderosaView</see>（コンテキストメニューの場合）のいずれかが渡されます。
    /// </para>
    /// <para>
    /// このデリゲートからの戻り値は、メニューにチェックを付けるのかどうかを判断するのに使われます。 <seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso>を参照してください。
    /// </para>
    /// </ja>
    /// <en>
    /// The return value from this delegate is used to judge whether to put the check on the menu. Refer to <seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso>  
    /// </en>
    /// </remarks>
    public delegate bool CheckedDelegate(ICommandTarget target);

    /// <summary>
    /// <ja>
    /// メニューやツールバーボタンがイネーブルかディスエブルかを決めるときに呼び出されるデリゲートです。
    /// </ja>
    /// <en>
    /// Delegate called when whether menu and toolbar button are enable or disable is decided
    /// </en>
    /// </summary>
    /// <param name="target">
    /// <ja>
    /// コマンドの対象を示すターゲットです。
    /// </ja>
    /// <en>
    /// Target that shows object of command.
    /// </en>
    /// </param>
    /// <returns>
    /// <ja>
    /// メニューやツールバーのボタンが選択できるならtrueを、そうでないならfalseを返してください。
    /// </ja>
    /// <en>
    ///  Please return true and return false if it is not so if you can select the button of the menu and the toolbar. 
    /// </en>
    /// </returns>
    /// <remarks>
    /// <ja>
    /// <para>
    /// <paramref name="target">target</paramref>にはアクティブウィンドウを示す<see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>（メインメニューやツールバーの場合）またはアクティブビューを示す<see cref="Poderosa.Sessions.IPoderosaView">IPoderosaView</see>（コンテキストメニューの場合）のいずれかが渡されます。
    /// </para>
    /// <para>
    /// このデリゲートからの戻り値は、メニューやツールボタンをイネーブルにするかディスエブルにするかを定めるのに使われます。<seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso>や<seealso cref="Poderosa.Forms.ToolBarElementImpl">ToolBarElementImpl</seealso>を参照してください。
    /// </para>
    /// </ja>
    /// <en>
    /// The return value from this Derigat is used to provide whether to make the menu and the tool button enable or to make it to disable. Refer to <seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso> or <seealso cref="Poderosa.Forms.ToolBarElementImpl">ToolBarElementImpl</seealso>.
    /// </en>
    /// </remarks>
    public delegate bool EnabledDelegate(ICommandTarget target);

    /// <summary>
    /// <ja>
    /// コマンドが実行可能かどうかを定めるときに呼び出されるデリゲートです。
    /// </ja>
    /// <en>
    /// Delegate called when it is provided whether command is executable.
    /// </en>
    /// </summary>
    /// <param name="target">
    /// <ja>
    /// コマンドの対象を示すターゲットです。
    /// </ja>
    /// <en>
    /// Target that shows object of command.
    /// </en>
    /// </param>
    /// <returns>
    /// <ja>
    /// コマンドを実行可能ならtrueを、そうでないならfalseを返してください。
    /// </ja>
    /// <en>
    /// Return true if it is executable, false if it is not.
    /// </en>
    /// </returns>
    /// <remarks>
    /// <ja>
    /// <para>
    /// メニューやツールバーから呼び出される場合、<paramref name="target">target</paramref>にはアクティブウィンドウを示す<see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>（メインメニューやツールバーの場合）またはアクティブビューを示す<see cref="Poderosa.Sessions.IPoderosaView">IPoderosaView</see>（コンテキストメニューの場合）のいずれかが渡されます。
    /// </para>
    /// <para>
    /// このデリゲートは<seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso>や<seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso>などで、<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
    /// <see cref="IPoderosaCommand.CanExecute">CanExecuteメソッド</see>が呼び出されるタイミングで呼び出されます。
    /// </para>
    /// </ja>
    /// <en>This delegatee is called in <seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso> and <seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso>, etc. according to timing where the <see cref="IPoderosaCommand.CanExecute">CanExecute method</see> of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> is called. 
    /// </en>
    /// </remarks>
    public delegate bool CanExecuteDelegate(ICommandTarget target);


    /// <exclude/>
    public delegate CommandResult ExecuteDelegateArgs(ICommandTarget target, params IAdaptable[] args);


    /// <summary>
    /// <ja>
    /// コマンドが実行されるときに呼び出されるデリゲートです。
    /// </ja>
    /// <en>
    /// Delegate called when command is executed
    /// </en>
    /// </summary>
    /// <param name="target">
    /// <ja>
    /// コマンドの対象を示すターゲットです。
    /// </ja>
    /// <en>
    /// Target that shows object of command.
    /// </en>
    /// </param>
    /// <returns>
    /// <ja>
    /// コマンドの実行結果を返してください。
    /// </ja>
    /// <en>
    /// Please return the execution result of the command. 
    /// </en>
    /// </returns>
    /// <remarks>
    /// <ja>
    /// <para>
    /// メニューやツールバーから呼び出される場合、<paramref name="target">target</paramref>にはアクティブウィンドウを示す<see cref="T:Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>（メインメニューやツールバーの場合）またはアクティブビューを示す<see cref="T:Poderosa.Sessions.IPoderosaView">IPoderosaView</see>（コンテキストメニューの場合）のいずれかが渡されます。
    /// </para>
    /// <para>
    /// このデリゲートは<seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso>や<seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso>などで、<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
    /// <see cref="IPoderosaCommand.InternalExecute">InternalExecuteメソッド</see>が呼び出されるタイミングで呼び出されます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// Either of <see cref="Poderosa.Sessions.IPoderosaView">IPoderosaView</see> (For the context menu) that shows 
    /// <see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see> (For the main menu and the toolbar) 
    /// that shows the active window or an active view is passed to <paramref name="target">target</paramref>
    ///  when it is called from the menu and the toolbar. 
    /// </para>
    /// <para>
    /// This delegate is called in <seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso> and <seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso>, etc. according to timing where the <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
    /// <see cref="IPoderosaCommand.InternalExecute">InternalExecute method</see> of IPoderosaCommand is called. 
    /// </para>
    /// </en>
    /// </remarks>
    public delegate CommandResult ExecuteDelegate(ICommandTarget target);

    //コマンドの基底
    /// <summary>
    /// <ja>
    /// コマンド機能を提供するプラグインが実装するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that plug-in that offers command function implements.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// コマンドマネージャ（<seealso cref="T:Poderosa.Commands.ICommandManager">ICommandManager</seealso>）によって実行されるコマンドを提供する場合には、プラグインがこのインターフェイスを実装します。
    /// </para>
    /// <para>
    /// コマンドマネージャの<see cref="M:Poderosa.Commands.ICommandManager.Execute(Poderosa.Commands.IPoderosaCommand,Poderosa.Commands.ICommandTarget,Poderosa.IAdaptable[])">Executeメソッド</see>を呼び出すと、このインターフェイスに実装されている
    /// <see cref="M:Poderosa.Commands.IPoderosaCommand.InternalExecute(Poderosa.Commands.ICommandTarget,Poderosa.IAdaptable[])">InternalExecuteメソッド</see>が間接的に呼び出されます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// When the command executed by the command manager(<seealso cref="ICommandManager">ICommandManager</seealso>) is offered, 
    /// the plug-in implements this interface. 
    /// </para>
    /// <para>
    /// When command manager's <see cref="ICommandManager.Execute">Execute method</see> is called, 
    /// the <see cref="IPoderosaCommand.InternalExecute">InternalExecute method </see> implemented on this interface is indirectly called. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IPoderosaCommand : IAdaptable {
        //ユーザがこれを直接呼んではいけない。CommandManager#Executeを使うこと！
        /// <summary>
        /// <ja>
        /// コマンドが実行されるときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of call when command is executed
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>
        /// コマンドの対象となるターゲットです。
        /// </ja>
        /// <en>
        /// Target target for command.
        /// </en>
        /// <en>
        /// Target that shows object of command.
        /// </en>
        /// </param>
        /// <param name="args">
        /// <ja>
        /// コマンドに渡される任意の引数です。
        /// </ja>
        /// <en>
        /// It is an arbitrary argument passed to the command. 
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// コマンドが成功かしたかどうかを示す戻り値です。成功したときには<see cref="CommandResult.Succeeded">CommandResult.Succeeded</see>を返します。
        /// </ja>
        /// <en>
        /// It is a return value that shows whether it was that the command succeeds. 
        /// When succeeding, CommandResult.<see cref="CommandResult.Succeeded">CommandResult.Succeeded</see> is returned. 
        /// </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、コマンドマネージャ（<seealso cref="ICommandManager">ICommandManager</seealso>）の<see cref="ICommandManager.Execute">Executeメソッド</see>
        /// が呼び出されたときに、間接的に呼び出されます。開発者は、このメソッドを直接呼び出してはいけません。
        /// </para>
        /// <para>
        /// <paramref name="target">target</paramref>や<paramref name="args">args</paramref>は、<see cref="ICommandManager.Execute">Executeメソッド</see>の呼び出しで渡された引数がそのまま渡されます。
        /// </para>
        /// <para>
        /// <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>を使うと、<paramref name="target">target</paramref>をウィンドウやビューへと変換できます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When command manager(<seealso cref="ICommandManager">ICommandManager</seealso>)'s <see cref="ICommandManager.Execute">Execute method</see> is called, this method is indirectly called.
        /// The developer must not call this method directly. 
        /// </para>
        /// <para>
        /// As for <paramref name="target">target</paramref> and <paramref name="args">args</paramref>, the argument passed by calling 
        /// the <see cref="ICommandManager.Execute">Execute method</see> is passed as it is. 
        /// </para>
        /// <para>
        /// <paramref name="target">target</paramref> can be converted into the window and the view by using <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>. 
        /// </para>
        /// </en>
        /// </remarks>
        CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args); //Eclipseではここには引数があり、パラメータやコマンド起動元が取れる。が、これは意味的にICommandの実装が知っているべき内容だ

        //コマンドが実行可能かどうかの判定。実行してみるまでわからないようなときはとりあえずtrueを返すこと。
        /// <summary>
        /// <ja>
        /// コマンドが実行可能かどうかを返します。
        /// </ja>
        /// <en>
        /// Return whether the command is executable.
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>
        /// コマンドの実行対象となるターゲットです。
        /// </ja>
        /// <en>
        /// Target that shows object of command.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 実行可能ならtrue、そうでないならfalseを返してください。
        /// </ja>
        /// <en>
        /// Return true if it is executable, false if it is not.
        /// </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、メニューやツールバーが、項目をディスエブルにするかどうかを決めるときに使われます。
        /// </para>
        /// <para>
        /// falseを返すとディスエブルになり、ユーザーが選択できなくなります。
        /// </para>
        /// <para>
        /// コマンドを実行するまで、実行可能かどうかがわからないときには、trueを返すように実装してください。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When it is decided whether the menu and the toolbar make the item disable, this method is used. 
        /// </para>
        /// <para>
        /// It becomes disable if false is returned, and the user cannot select it. 
        /// </para>
        /// <para>
        /// Please implement to return true when it is executable until the command is executed is not understood. 
        /// </para>
        /// </en>
        /// </remarks>
        bool CanExecute(ICommandTarget target);
    }

    //メインメニューからたどれるタイプのやつ
    /// <summary>
    /// <ja>
    /// コマンドマネージャによって管理されるコマンドを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows command managed by command manager
    /// </en>
    /// </summary>
    public interface IGeneralCommand : IPoderosaCommand {
        /// <summary>
        /// <ja>
        /// コマンドを内部で識別するための「コマンドID」です。他のコマンドとは重複しない一意のものを設定します。 
        /// </ja>
        /// <en>
        /// It is "command ID" to identify the command internally. The unique one that doesn't overlap is set as other commands. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 開発者がコマンドを提供する場合には、他のプラグインが提供するコマンドIDと重複しないようにするため、
        /// 「プラグインID」の下に適当な名前を付けた命名規則で「コマンドID」を決定することを推奨します。
        /// たとえば「<c>co.jp.example.myplugin</c>」というプラグインIDをもつプラグインならば、
        /// コマンドIDとして「<c>co.jp.example.myplugin.mycommand</c>」といった名前を付けるようにします。 
        /// </ja>
        /// <en>
        /// Command ID is recommended to be decided in the naming convention that names a suitable name under "plug-in ID" to make it not overlap with "command ID" that other plug-ins offer when the developer offers the command. 
        /// For instance, if it is a plug-in with plug-in ID of "<c>co.jp.example.myplugin</c>", the name of "<c>co.jp.example.myplugin.mycommand</c>" is named as command ID. 
        /// </en>
        /// </remarks>
        string CommandID {
            get;
        }
        /// <summary>
        /// <ja>
        /// コマンドの説明文です。
        /// </ja>
        /// <en>
        /// Explanation of command
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 設定した値は、オプション画面の「コマンド」欄に表示される文字列になります。
        /// </ja>
        /// <en>
        /// The set value becomes a character string displayed in "Command" column on the option screen. 
        /// </en>
        /// </remarks>
        string Description {
            get;
        }
        /// <summary>
        /// <ja>
        /// このコマンドに割り当てられるデフォルトのショートカットキーです。
        /// </ja>
        /// <en>
        /// It is a shortcut key of the default allocated in this command. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ショートカットキーを割り当てない場合には、<c>Keys.None</c>を渡してください。
        /// </para>
        /// <para>
        /// ショートカットが既存のコマンドが用いているものと重複する場合には、無視されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// Please pass <c>Keys.None</c> when you do not allocate the shortcut key. 
        /// </para>
        /// <para>
        /// When the short cut overlaps with the one that an existing command uses, it is disregarded. 
        /// </para>
        /// </en>
        /// </remarks>
        Keys DefaultShortcutKey {
            get;
        }
        /// <summary>
        /// <ja>
        /// コマンドカテゴリを示す<seealso cref="Poderosa.Commands.ICommandCategory">ICommandCategory</seealso>です。
        /// </ja>
        /// <en>
        /// <seealso cref="Poderosa.Commands.ICommandCategory">ICommandCategory</seealso> that show the command category.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="Poderosa.Commands.ICommandCategory">ICommandCategory</seealso>は、
        /// <seealso cref="Poderosa.Commands.ICommandManager">ICommandManager</seealso>の
        /// <see cref="Poderosa.Commands.ICommandManager.CommandCategories">CommandCategoriesプロパティ</see>から得た、
        /// 定義済みカテゴリを用いることもできます。
        /// </ja>
        /// <en>
        /// <seealso cref="Poderosa.Commands.ICommandCategory">ICommandCategory</seealso> can use the category that has been 
        /// defined obtaining it from the <see cref="Poderosa.Commands.ICommandManager.CommandCategories">CommandCategories property</see> 
        /// of <seealso cref="Poderosa.Commands.ICommandManager">ICommandManager</seealso>. 
        /// </en>
        /// </remarks>
        ICommandCategory CommandCategory {
            get;
        }
    }

    //PositionDesignationで
    /// <summary>
    /// <ja>
    /// コマンドカテゴリを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows command category.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 定義済みカテゴリは、<seealso cref="ICommandManager">ICommandManager</seealso>の<see cref="ICommandManager.CommandCategories">CommandCategoriesプロパティ</see>から取得できます。
    /// </ja>
    /// <en>
    /// It is possible to get it from the <see cref="ICommandManager.CommandCategories">CommandCategories property</see> of <seealso cref="ICommandManager">ICommandManager</seealso>. 
    /// </en>
    /// </remarks>
    public interface ICommandCategory : IAdaptable {
        /// <summary>
        /// <ja>
        /// コマンドカテゴリの名前です。
        /// </ja>
        /// <en>
        /// Name of the command category.
        /// </en>
        /// </summary>
        string Name {
            get;
        }
        /// <summary>
        /// <ja>
        /// キーバインドのカスタマイズが可能かどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether customizing key bind is possible. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// trueにするとオプション設定画面に、この項目が表示され、キーバインドの変更ができるようになります。
        /// </para>
        /// <para>
        /// falseにするとオプション設定画面から隠されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// This item is displayed on the option setting screen when making it to true, and it comes to be able to change key bind. 
        /// </para>
        /// <para>
        /// It is concealed because of the option setting screen when making it to false. 
        /// </para>
        /// </en>
        /// </remarks>
        bool IsKeybindCustomizable {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// 定義済みコマンドカテゴリを示します
    /// </ja>
    /// <en>
    /// Defined  command category.
    /// </en>
    /// </summary>
    public interface IDefaultCommandCategories {
        /// <summary>
        /// <ja>
        /// ［ファイル］を示すカテゴリです。
        /// </ja>
        /// <en>
        /// Category that shows the "File".
        /// </en>
        /// </summary>
        ICommandCategory File {
            get;
        }
        /// <summary>
        /// <ja>
        /// ［ダイアログ］を示すカテゴリです。
        /// </ja>
        /// <en>
        /// Category that shows the "Dialog".
        /// </en>
        /// </summary>
        ICommandCategory Dialogs {
            get;
        }
        /// <summary>
        /// <ja>
        /// ［編集］を示すカテゴリです。
        /// </ja>
        /// <en>
        /// Category that shows the "Edit".
        /// </en>
        /// </summary>
        ICommandCategory Edit {
            get;
        }
        /// <summary>
        /// <ja>
        /// ［ウィンドウ］を示すカテゴリです。
        /// </ja>
        /// <en>
        /// Category that shows the "Window".
        /// </en>
        /// </summary>
        ICommandCategory Window {
            get;
        }
    }

    //GeneralCommandのコレクション
    /// <summary>
    /// <ja>
    /// コマンドマネージャを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows command manager.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// コマンドマネージャは「<c>org.poderosa.core.commands</c>」というプラグインIDをもつCommandManagerPluginプラグインによって
    /// 提供されています。
    /// </para>
    /// <para>
    /// このインターフェイスを取得するには、（1）<seealso cref="Poderosa.Plugins.IPluginManager">IPluginManager</seealso>の
    /// <see cref="Poderosa.Plugins.IPluginManager.FindPlugin">FindPluginメソッド</see>で「<c>org.poderosa.core.commands</c>」を検索する、
    /// （2）<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>の<see cref="Poderosa.Plugins.ICoreServices.CommandManager">CommandManagerプロパティ</see>を使って取得する、のいずれかの方法がとれます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The command manager is offered by CommandManagerPlugin plug-in with plug-in ID of "<c>org.poderosa.core.commands</c>".
    /// </para>
    /// <para>
    /// To get this interface, the developer cat adopt either of method. (1) Retrieve [<c>org.poderosa.core.commands</c>] by <see cref="Poderosa.Plugins.IPluginManager.FindPlugin">FindPlugin method</see> on <seealso cref="Poderosa.Plugins.IPluginManager">IPluginManager</seealso>.
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// ICoreServiceのCommandManagerプロパティからICommandManagerを取得します。
    /// <code>
    /// // ここでPoderosaWorldは、InitializePluginメソッドで受け取った
    /// // IPoderosaWorldであると仮定します。
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// ICommandManager cm = cs.CommandManager;
    /// </code>
    /// </ja>
    /// <en>
    /// Get ICommandManager from CommandManager property on ICoreService.
    /// <code>
    /// // It is assumed that PoderosaWorld is IPoderosaWorld here received by the InitializePlugin method. 
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// ICommandManager cm = cs.CommandManager;
    /// </code>
    /// </en>
    /// </example>
    public interface ICommandManager : IAdaptable {
        /// <summary>
        /// <ja>
        /// コマンドをコマンドマネージャに登録します。
        /// </ja>
        /// <en>
        /// Regist command to the command manager.
        /// </en>
        /// </summary>
        /// <param name="command">
        /// <ja>
        /// 登録したいコマンドです。
        /// </ja>
        /// <en>
        /// Command to be regist.
        /// </en>
        /// </param>
        void Register(IGeneralCommand command);
        /// <summary>
        /// <ja>
        /// コマンドIDをキーにして、コマンドマネージャに登録されたコマンドを検索します。
        /// </ja>
        /// <en>
        /// Retrieve command ID is made a key, and the command registered by the command manager.
        /// </en>
        /// </summary>
        /// <param name="id">
        /// <ja>
        /// 検索するコマンドIDです。
        /// </ja>
        /// <en>
        /// Retrieval of the command ID.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 見つかったコマンドオブジェクトの<seealso cref="IGeneralCommand">IGeneralCommand</seealso>が返されます。
        /// 見つからなかったときには<c>null</c>が返されます。
        /// </ja>
        /// <en>
        /// <seealso cref="IGeneralCommand">IGeneralCommand</seealso> of the found command object is returned. 
        /// When not found, <c>null</c> is returned. 
        /// </en>
        /// </returns>
        /// <overloads>
        /// <summary>
        /// <ja>コマンドを実行します。</ja>
        /// <en>Execute the command.</en>
        /// </summary>
        /// </overloads>
        IGeneralCommand Find(string id);
        /// <summary>
        /// <ja>
        /// ショートカットキーをキーにして、コマンドマネージャに登録されたコマンドを検索します。
        /// </ja>
        /// <en>
        /// The shortcut key is made a key, and the command registered by the command manager is retrieved. 
        /// </en>
        /// </summary>
        /// <param name="key">
        /// <ja>
        /// 検索するショートカットキーです。
        /// </ja>
        /// <en>
        /// Retrieval short cut key.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// 見つかったコマンドオブジェクトの<seealso cref="IGeneralCommand">IGeneralCommand</seealso>が返されます。
        /// 見つからなかったときには<c>null</c>が返されます。
        /// </ja>
        /// <en>
        /// <seealso cref="IGeneralCommand">IGeneralCommand</seealso> of the found command object is returned. 
        /// When not found, <c>null</c> is returned. 
        /// </en>
        /// </returns>
        IGeneralCommand Find(Keys key); //ショートカットキーから
        /// <summary>
        /// <ja>
        /// コマンドマネージャに登録されているすべてのコマンドオブジェクトを列挙します。
        /// </ja>
        /// <en>
        /// Enumerate all the command objects being registered by the command manager.
        /// </en>
        /// </summary>
        IEnumerable<IGeneralCommand> Commands {
            get;
        }

        /// <summary>
        /// <ja>
        /// 指定されたコマンドを実行します。
        /// </ja>
        /// <en>
        /// Execute the specified command.
        /// </en>
        /// </summary>
        /// <param name="command">
        /// <ja>
        /// 実行するコマンドです。
        /// </ja>
        /// <en>
        /// Command to execute.
        /// </en>
        /// </param>
        /// <param name="target">
        /// <ja>
        /// コマンドのターゲットです。
        /// </ja>
        /// <en>
        /// Target of command.
        /// </en>
        /// </param>
        /// <param name="args">
        /// <ja>
        /// コマンドに渡す任意の引数です。
        /// </ja>
        /// <en>
        /// Arbitrary argument passed to command
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// コマンドの実行結果です。この値は、<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
        /// <see cref="IPoderosaCommand.InternalExecute">InternalExecuteメソッド</see>が返す値と同じです。
        /// </ja>
        /// <en>
        /// It is an execution result of the command. This value is the same as the value that the 
        /// <see cref="IPoderosaCommand.InternalExecute">InternalExecute method</see>
        ///  of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> returns. 
        /// </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// メニューやツールバーから呼び出されるコマンドは、<paramref name="target"/>には、
        /// メインウィンドウを示す<see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>が渡されることを想定しています。この種以外のインターフェイスが渡されたときには、
        /// 正しく動作しません。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// The command called from the menu and the toolbar assumes to <paramref name="target"/> <see cref="Poderosa.Forms.IPoderosaMainWindow">IPoderosaMainWindow</see>'s that shows the main window being passed. 
        /// When interfaces other than this seed are passed, it doesn't operate correctly. 
        /// </para>
        /// </en>
        /// </remarks>
        /// <example>
        /// <ja>
        /// 「ファイルへコピー」の機能を実装しているコマンド「<c>org.poderosa.terminalemulator.copytofile</c>」を呼び出して、
        /// 現在選択されている範囲をファイルへとコピーします。
        /// <code>
        /// // ICoreServicesの取得
        /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
        /// // コマンドマネージャの取得
        /// ICommandManager cm = cs.CommandManager;
        /// 
        /// // 「ファイルへコピー」のコマンドを検索
        /// IGeneralCommand cmd = cm.Find("org.poderosa.terminalemulator.copytofile");
        /// 
        /// // アクティブウィンドウのIPoderosaMainWindowを得る
        /// IPoderosaMainWindow mainwin = cs.WindowManager.ActiveWindow;
        /// 
        /// // 実行
        /// CommandResult result = cm.Execute(cmd, mainwin);
        /// </code>
        /// </ja>
        /// <en>
        /// Command "<c>org.poderosa.terminalemulator.copytofile</c>" where the function of "Save to file" is implemented is called, 
        /// and the range that has been selected now is copied to the file. 
        /// <code>
        /// // Get ICoreServices
        /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
        /// // Get command manager.
        /// ICommandManager cm = cs.CommandManager;
        /// 
        /// // Retrieval of the command of "Save to file"
        /// IGeneralCommand cmd = cm.Find("org.poderosa.terminalemulator.copytofile");
        /// 
        /// // Get the IPoderosaMainWindow of the active window.
        /// IPoderosaMainWindow mainwin = cs.WindowManager.ActiveWindow;
        /// 
        /// // Execute
        /// CommandResult result = cm.Execute(cmd, mainwin);
        /// </code>
        /// </en>
        /// </example>
        CommandResult Execute(IPoderosaCommand command, ICommandTarget target, params IAdaptable[] args);

        /// <exclude/>
        IKeyBinds CurrentKeyBinds {
            get;
        }

        /// <exclude/>
        IKeyBinds GetKeyBinds(IPreferenceFolder folder); //ちょっと違和感。別インタフェースに分ける？

        /// <summary>
        /// <ja>
        /// 定義済みコマンドカテゴリを取得するためのインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface to get command category that has been defined.
        /// </en>
        /// </summary>
        IDefaultCommandCategories CommandCategories {
            get;
        }
    }

    //キーバインド設定
    /// <summary>
    /// <ja>
    /// キーバインドの設定を操作するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that operates setting of key bind.
    /// </en>
    /// </summary>
    public interface IKeyBinds {
        /// <summary>
        /// <ja>
        /// 割り当てられているショートカットキーのコレクションです。
        /// </ja>
        /// <en>
        /// Collection of allocated shortcut key.
        /// </en>
        /// </summary>
        ICollection Commands {
            get;
        }

        /// <summary>
        /// <ja>
        /// コマンドに割り当てられたショートカットキーを返します。
        /// </ja>
        /// <en>
        /// Return the shortcut key allocated in the command.
        /// </en>
        /// </summary>
        /// <param name="command">
        /// <ja>
        /// 調べたいコマンドです。
        /// </ja>
        /// <en>
        /// Command that wants to be examined.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// キーに割り当てられたショートカットキーが戻ります。ショートカットキーが割り当てられていない場合には、Keys.Noneが戻ります。
        /// </ja>
        /// <en>
        /// The shortcut key allocated in the key returns. Keys.None returns when the shortcut key is not allocated. 
        /// </en>
        /// </returns>
        Keys GetKey(IGeneralCommand command);
        /// <summary>
        /// <ja>
        /// コマンドに対してショートカットキーを割り当てます。
        /// </ja>
        /// <en>
        /// The shortcut key is allocated to the command. 
        /// </en>
        /// </summary>
        /// <param name="command">
        /// <ja>
        /// 対象となるコマンドです。
        /// </ja>
        /// <en>
        /// Command that becomes object.
        /// </en>
        /// </param>
        /// <param name="key">
        /// <ja>
        /// 割り当てるショートカットキーです。
        /// </ja>
        /// <en>
        /// Allocated shortcut key
        /// </en>
        /// </param>
        /// <exception cref="ArgumentException">
        /// <ja>
        /// 該当のキーには、すでにほかのコマンドが割り当てられています。
        /// </ja>
        /// <en>
        /// Other commands have already been allocated in the key to the correspondence. 
        /// </en>
        /// </exception>
        /// <remarks>
        /// <ja>
        /// <para>
        /// <paramref name="key">key</paramref>にKeys.Noneを渡すと、ショートカットキーの割り当てを解除できます。
        /// </para>
        /// <para>
        /// <paramref name="key">key</paramref>にすでにコマンドが割り当てられているときには例外が発生します。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// The allocation of the shortcut <paramref name="key">key</paramref> can be released by passing key Keys.None. 
        /// </para>
        /// <para>
        /// When the command has already been allocated in <paramref name="key">key</paramref>, the exception is generated. 
        /// </para>
        /// </en>
        /// </remarks>
        void SetKey(IGeneralCommand command, Keys key);

        /// <summary>
        /// <ja>
        /// ショートカットキーに割り当てられているコマンドを検索します。
        /// </ja>
        /// <en>
        /// Retrieval the command allocated in the shortcut key.
        /// </en>
        /// </summary>
        /// <param name="key">
        /// <ja>
        /// 検索するショートカットキーです。
        /// </ja>
        /// <en>
        /// Retrieved shortcut key
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>ショートカットキーに割り当てられているコマンドが返されます。見つからないときにはnullが返されます。</ja>
        /// <en>The command allocated in the shortcut key is returned. When not found, null is returned. </en>
        /// </returns>
        IGeneralCommand FindCommand(Keys key);

        /// <summary>
        /// <ja>
        /// ショートカットキーの割り当てをすべてクリアします。
        /// </ja>
        /// <en>
        /// The allocation of the shortcut key is all cleared. 
        /// </en>
        /// </summary>
        void ClearAll();
        /// <summary>
        /// <ja>
        /// ショートカットキーの割り当てをデフォルトに戻します。
        /// </ja>
        /// <en>
        /// The allocation of the shortcut key is set to default.
        /// </en>
        /// </summary>
        void ResetToDefault();
        /// <summary>
        /// <ja>
        /// ショートカットキーの割り当てをインポートします。
        /// </ja>
        /// <en>
        /// Import the allocation of the shortcut key.
        /// </en>
        /// </summary>
        /// <param name="keybinds">
        /// <ja>
        /// インポートするショートカットキーです。
        /// </ja>
        /// <en>
        /// The shortcut key to import.
        /// </en>
        /// </param>
        void Import(IKeyBinds keybinds);

    }

    //メニュー項目

    //Extension Pointへの接続用
    /// <summary>
    /// <ja>
    /// メニューの個々のアイテムを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows each item of menu.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// メニュー項目を作成する場合には、<seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemTmpl</seealso>を使います。
    /// </ja>
    /// <en>
    /// When the menu item is made, <seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemTmpl</seealso> is used. 
    /// </en>
    /// </remarks>
    public interface IPoderosaMenu : IAdaptable {
        /// <summary>
        /// <ja>
        /// メニューに表示されるテキストです。
        /// </ja>
        /// <en>
        /// Text displayed in menu
        /// </en>
        /// </summary>
        string Text {
            get;
        }
        /// <summary>
        /// <ja>
        /// メニューのイネーブル／ディスエブルの状態を返すメソッドです。
        /// </ja>
        /// <en>
        /// Method of returning state of enable/disable of menu
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>
        /// コマンドのターゲットです。
        /// </ja>
        /// <en>
        /// Target of command.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// メニューがイネーブルである場合にはtrue、ディスエブルである場合にはfalseが返されます。
        /// </ja>
        /// <en>
        /// When it is true, and disable when the menu is enable, false is returned. 
        /// </en>
        /// </returns>
        bool IsEnabled(ICommandTarget target);
        /// <summary>
        /// <ja>
        /// メニューのチェック状態を返すメソッドです。
        /// </ja>
        /// <en>
        /// Method of returning check state on menu
        /// </en>
        /// </summary>
        /// <param name="target">
        /// <ja>
        /// コマンドのターゲットです。
        /// </ja>
        /// <en>
        /// Target of command.
        /// </en>
        /// </param>
        /// <returns>
        /// <ja>
        /// メニューにチェックが付いている場合にはtrue、そうでない場合にはfalseが返されます。
        /// </ja>
        /// <en>
        /// When the menu is checked, true is returned when it is false so. 
        /// </en>
        /// </returns>
        bool IsChecked(ICommandTarget target);
    }

    //MenuGroupはバーのデリミタが入る単位
    /// <summary>
    /// <ja>
    /// メニュー項目を集めたメニューグループを構成するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that composes menu group that collects menu items.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// メニューをPoderosaに登録する場合には、メニュー項目を集めたメニューグループを作成し、拡張ポイントへと登録します。
    /// </para>
    /// <para>
    /// メニューグループを作成する場合には、<seealso cref="PoderosaMenuGroupImpl">PoderosaMenuGroupImpl</seealso>を使うことができます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The menu group that collects the menu items is made when the menu is registered in Poderosa, and it registers to the extension point. 
    /// </para>
    /// <para>
    /// When the menu group is made, <seealso cref="PoderosaMenuGroupImpl">PoderosaMenuGroupImpl</seealso> can be used. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IPoderosaMenuGroup : IAdaptable {
        /// <summary>
        /// <ja>
        /// このメニューグループに含まれるメニュー項目の配列です。
        /// </ja>
        /// <en>
        /// Array of menu item included in this menu group.
        /// </en>
        /// </summary>
        IPoderosaMenu[] ChildMenus {
            get;
        }
        /// <summary>
        /// <ja>
        /// メニュー項目が動的に作られるかどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the menu item is dynamically made. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// trueである場合、メニュー項目が表示されようとするたびに、メニューが再生成されます。動的なメニューを構成する場合にはtrueを、そうでない場合にはfalseを返すように実装します。
        /// </ja>
        /// <en>
        /// The menu is done whenever the menu item tries to be displayed when it is true and the reproduction is done. 
        /// When a dynamic menu is composed, true is implemented to return false when it is not so. 
        /// </en>
        /// </remarks>
        bool IsVolatileContent {
            get;
        }
        /// <summary>
        /// <ja>
        /// このメニューグループの前に区切り記号（セパレータ）が入るかどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the separator enters ahead of this menu group. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// trueである場合、このメニューグループの直前に区切り記号（セパレータ）が表示されます。
        /// </ja>
        /// <en>
        /// When it is true, the separator is displayed just before this menu group. 
        /// </en>
        /// </remarks>
        bool ShowSeparator {
            get;
        } //グループの前にセパレータが入るかどうか
    }

    /// <summary>
    /// <ja>
    /// メニューを階層化するためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to hierarchize menu
    /// </en>
    /// </summary>
    public interface IPoderosaMenuFolder : IPoderosaMenu {
        /// <summary>
        /// <ja>
        /// 階層化したサブメニューの配列です。
        /// </ja>
        /// <en>
        /// Array of hierarchized submenu
        /// </en>
        /// </summary>
        IPoderosaMenuGroup[] ChildGroups {
            get;
        }
    }


    /// <summary>
    /// <ja>
    /// 実行されるときに引数を伴わないメニュー項目を示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows menu item not to accompany argument when executed.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このメニュー項目は、<seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso>を使うことで作成できます。
    /// </ja>
    /// <en>
    /// This menu item can be made by using <seealso cref="PoderosaMenuItemImpl">PoderosaMenuItemImpl</seealso>. 
    /// </en>
    /// </remarks>
    public interface IPoderosaMenuItem : IPoderosaMenu {
        /// <summary>
        /// <ja>
        /// メニューが選択されたときに呼び出されるコマンドです。
        /// </ja>
        /// <en>
        /// Command called when menu is selected.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// メニューが選択されると、このプロパティで設定した<see cref="IPoderosaCommand.InternalExecute">InternalExecuteメソッド</see>
        /// が呼び出されます。
        /// </ja>
        /// <en>
        /// The <see cref="IPoderosaCommand.InternalExecute">InternalExecute method</see> that sets for the menu to be selected in this property is called. 
        /// </en>
        /// </remarks>
        IPoderosaCommand AssociatedCommand {
            get;
        }
    }

    //MRUなど、引数付きのやつ
    /// <summary>
    /// <ja>
    /// 実行されるときに引数を伴うメニュー項目を示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows menu item with argument when executed
    /// </en>
    /// </summary>
    public interface IPoderosaMenuItemWithArgs : IPoderosaMenuItem {
        /// <summary>
        /// <ja>
        /// コマンドが実行されるときに引き渡す任意の引数です。
        /// </ja>
        /// <en>
        /// Arbitrary argument handed over when command is executed.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// この引数は、<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
        /// <see cref="IPoderosaCommand.InternalExecute">InternalExecuteメソッド</see>
        /// が呼び出されるとき、第3引数にそのまま渡されます。
        /// </ja>
        /// <en>
        /// When the <see cref="IPoderosaCommand.InternalExecute">InternalExecute method</see> of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> is called, this argument is passed to the third argument as it is. 
        /// </en>
        /// </remarks>
        IAdaptable[] AdditionalArgs {
            get;
        }
    }

    //コンテキストメニューを供給する能力のあるクラスが実装
    /// <summary>
    /// <ja>
    /// コンテキストメニューの機能をもつプラグインが実装すべきクラスです。
    /// </ja>
    /// <en>
    /// Class that plug-in with function of context menu should implement.
    /// </en>
    /// </summary>
    public interface IPoderosaContextMenuPoint : IAdaptable {
        //nullも可
        /// <summary>
        /// <ja>
        /// コンテキストメニューを示すメニューグループです。
        /// </ja>
        /// <en>
        /// Menu group that shows context menu
        /// </en>
        /// </summary>
        IPoderosaMenuGroup[] ContextMenu {
            get;
        }
    }



    //IPoderosaCommand標準実装
    /// <summary>
    /// <ja>
    /// <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>を実装したクラスです。コマンドを作成する際に使います。
    /// </ja>
    /// <en>
    /// It is a class that implements <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>. When the command is made, it uses it. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// コマンドを実装する開発者は、このクラスを使うことで<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>を実装したオブジェクト
    /// を容易に作成できます。
    /// </para>
    /// <para>
    /// ショートカットキーを割り当てるコマンドを作成する場合には、<seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso>を使ってください。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer who implements the command can easily make the object where <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> is implemented by using this class. 
    /// </para>
    /// <para>
    /// Please use <seealso cref="GeneralCommandImpl">GeneralCommandImpl</seealso> when you make the command that allocates the shortcut key. 
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>を実装したオブジェクトは、次のようにして作成できます。
    /// <code>
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行されたときの処理
    ///    MessageBox.Show("実行されました");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行できるかどうかを返す
    ///    return true;
    ///  }
    /// );
    /// </code>
    /// </ja>
    /// <en>
    /// The object that implements <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> can be made as follows. 
    /// <code>
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // Processing when command is executed
    ///    MessageBox.Show("Executed");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // Return whether the command can be executed. 
    ///    return true;
    ///  }
    /// );
    /// </code>
    /// </en>
    /// </example>
    public class PoderosaCommandImpl : IPoderosaCommand {
        /// <exclude/>
        protected ExecuteDelegate _execute;
        /// <exclude/>
        protected CanExecuteDelegate _canExecute;

        /// <summary>
        /// <ja>
        /// 引数なしのコンストラクタです。
        /// </ja>
        /// <en>
        /// Constructor that doesn't have argument
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このコンストラクタで作成されたコマンドは、コマンドが実行されてもいかなる処理もしません。
        /// </ja>
        /// <en>
        /// The command made by this constructor is executed the command or doesn't do the becoming it processing either. 
        /// </en>
        /// </remarks>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// コマンドオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// Making the command object. 
        /// </en>
        /// </summary>
        /// </overloads>
        public PoderosaCommandImpl() {
            _execute = null;
            _canExecute = null;
        }
        /// <summary>
        /// <ja>
        /// コマンドが実行される際に呼び出すデリゲートを指定したコンストラクタです。
        /// </ja>
        /// <en>
        /// Constructor who specified delegate called when command is executed
        /// </en>
        /// </summary>
        /// <param name="execute">
        /// <ja>
        /// コマンドが実行される際に呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed
        /// </en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// コマンドが実行される際――言い換えると<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
        /// <see cref="IPoderosaCommand.InternalExecute">InternalExecute</see>メソッドが呼び出されるときに、<paramref name="execute">execute</paramref>
        /// に指定したデリゲートが呼び出されます。
        /// </para>
        /// <para>
        /// <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の<see cref="IPoderosaCommand.CanExecute">CanExecute</see>メソッドの処理では、常にtrueが返されるように実装されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="IPoderosaCommand.InternalExecute">InternalExecute</see> method of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> is called, specified delegate is called in execute when paraphrasing it when the command is executed. 
        /// </para>
        /// <para>
        /// In the processing of the <see cref="IPoderosaCommand.CanExecute">CanExecute</see> method of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>, true is always implemented so that it is returned. 
        /// </para>
        /// </en>
        /// </remarks>
        public PoderosaCommandImpl(ExecuteDelegate execute) {
            _execute = execute;
            _canExecute = null;
        }
        /// <summary>
        /// <ja>
        /// コマンドが実行される際に呼び出すデリゲートと、メニューやツールバーが選択可能かどうかを示すデリゲートを指定したコンストラクタです。
        /// </ja>
        /// <en>
        /// Constructor that specified delegate that shows whether delegate, menu, and toolbar called when command is executed can be selected
        /// </en>
        /// </summary>
        /// <param name="execute">
        /// <ja>
        /// コマンドが実行される際に呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed.
        /// </en>
        /// </param>
        /// <param name="canExecute">
        /// <ja>メニューやツールバーをイネーブルにするかディスエブルにするかを決めるときに呼び出されるデリゲートです。</ja>
        /// <en>Delegate called when whether menu and toolbar are made Inabl or making to disable is decided.</en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// コマンドが実行される際――言い換えると<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の
        /// <see cref="IPoderosaCommand.InternalExecute">InternalExecute</see>メソッドが呼び出されるときに、<paramref name="execute">execute</paramref>
        /// に指定したデリゲートが呼び出されます。
        /// </para>
        /// <para>
        /// メニューやツールバーをイネーブルにするかディスエブルにするかを決めるとき――
        /// 言い換えると<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の<see cref="IPoderosaCommand.CanExecute">CanExecute</see>メソッドが呼び出されるときに、
        /// <paramref name="canExecute">canExecute</paramref>に指定したデリゲートが呼び出されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="IPoderosaCommand.InternalExecute">InternalExecute</see> method of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> is called, specified delegate is called in execute when paraphrasing it when the command is executed. 
        /// </para>
        /// <para>
        /// In the processing of the <see cref="IPoderosaCommand.CanExecute">CanExecute</see> method of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>, true is always implemented so that it is returned. 
        /// </para>
        /// </en>
        /// </remarks>
        public PoderosaCommandImpl(ExecuteDelegate execute, CanExecuteDelegate canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// <ja>
        /// コンストラクタで指定されたデリゲートを実行するためのオーバーライドされています。
        /// </ja>
        /// <en>
        /// The override to execute delegate specified by the constructor is done. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>コマンドのターゲットです。</ja>
        /// <en>
        /// Target of command.
        /// </en>
        /// </param>
        /// <param name="args"><ja>コマンドの引数です。</ja>
        /// <en>Argument of commane.</en></param>
        /// <returns><ja>コンストラクタで指定されたデリゲートを実行した結果が戻されます。</ja>
        /// <en>The result of executing delegate specified by the constructor is returned. </en></returns>
        /// <remarks>
        /// <ja>
        /// 引数なしのコンストラクタでこのオブジェクトが作られた場合、何も実行されることはなく、戻り値としてCommandResult.Ignoredが返されます。
        /// </ja>
        /// <en>
        /// Nothing is executed when this object is made from the constructor who doesn't have the argument, and CommandResult.Ignored is returned as a return value. 
        /// </en>
        /// </remarks>
        public virtual CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            return _execute == null ? CommandResult.Ignored : _execute(target);
        }

        /// <summary>
        /// <ja>
        /// コンストラクタで指定されたデリゲートを実行するためにオーバーライドされています。
        /// </ja>
        /// <en>
        /// To execute delegate specified by the constructor, override is done. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>コマンドのターゲットです。</ja>
        /// <en>
        /// Target of command.
        /// </en>
        /// </param>
        /// <returns><ja>コンストラクタで指定されたデリゲートを実行した結果が戻されます。</ja>
        /// <en>The result of executing delegate specified by the constructor is returned. </en></returns>
        /// <remarks>
        /// <ja>
        /// 引数なし、または、引数が1つのコンストラクタでこのオブジェクトが作られた場合、常にtrueが返されます。
        /// </ja>
        /// <en>
        /// The argument none or the argument is returned and when this object is made from one constructor, true is always returned. 
        /// </en>
        /// </remarks>
        public virtual bool CanExecute(ICommandTarget target) {
            return _canExecute == null ? true : _canExecute(target);
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return CommandManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    //IGeneralCommand標準実装
    /// <summary>
    /// <ja>
    /// <seealso cref="IGeneralCommand">IGeneralCommand</seealso>を実装したクラスです。ショートカットキーを割り当てるコマンドを作成する際に使います。
    /// </ja>
    /// <en>
    /// It is a class that implements <seealso cref="IGeneralCommand">IGeneralCommand</seealso>. When the command that allocates the shortcut key is made, it uses it. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ショートカットキーを割り当てるコマンドを実装する開発者は、このクラスを使うことで<seealso cref="IGeneralCommand">IGeneralCommand</seealso>を実装した
    /// オブジェクトを容易に作成できます。
    /// </para>
    /// <para>
    /// ショートカットキーを割り当てる必要がない場合には、<seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso>を使ってください。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer who implements the command that allocates the shortcut key can easily make the object where <seealso cref="IGeneralCommand">IGeneralCommand</seealso> is implemented by using this class. 
    /// </para>
    /// <para>
    /// Please use <seealso cref="PoderosaCommandImpl">PoderosaCommandImpl</seealso> when you need not allocate the shortcut key. 
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// [編集］という定義済みカテゴリに属するコマンドオブジェクトを作成するには、次のようにします。
    /// <code>
    /// // コマンドマネージャの取得
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// ICommandManager cm = cs.CommandManager;
    /// 
    /// // コマンド作成
    /// IGeneralCommand mycmd = new GeneralCommandImpl(
    ///   "co.example.myplugin.mycommand",
    ///  "MyCommand", cm.CommandCategories.Edit,
    ///   delegate(ICommandTarget target)
    ///  {
    ///     // ここにコマンドが実行されるときの処理を記述します
    ///   return CommandResult.Succeeded; // 成功ならSucceededを返す
    ///  },
    ///  delegate(ICommandTarget target)
    ///  {
    ///     // ここにコマンドの可否が調べられるときの処理を記述します
    ///     return true; // 実行可能ならtrueを返す
    ///  }
    /// );
    /// </code>
    /// 
    /// </ja>
    /// <en>
    /// To make the command object that belongs to the definition ending category of "Edit", as follows is done. 
    /// <code>
    /// // Get command manager.
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// ICommandManager cm = cs.CommandManager;
    /// 
    /// // Make the command.
    /// IGeneralCommand mycmd = new GeneralCommandImpl(
    ///   "co.example.myplugin.mycommand",
    ///  "MyCommand", cm.CommandCategories.Edit,
    ///   delegate(ICommandTarget target)
    ///  {
    ///     // The processing when the command is executed here is described. 
    ///   return CommandResult.Succeeded; // If it is a success, Succeeded is returned. 
    ///  },
    ///  delegate(ICommandTarget target)
    ///  {
    ///     // The processing when right or wrong of the command is examined here is described. 
    ///     return true; // If it is executable, true is returned. 
    ///  }
    /// );
    /// </code>
    /// 
    /// </en>
    /// </example>
    public class GeneralCommandImpl : IGeneralCommand {
        /// <exclude/>
        protected string _commandID;
        /// <exclude/>
        protected string _descriptionTextID;
        /// <exclude/>
        protected StringResource _strResource;
        /// <exclude/>
        protected bool _usingStringResource;
        /// <exclude/>
        protected Keys _defaultShortcutKey;
        /// <exclude/>
        protected ICommandCategory _commandCategory;
        /// <exclude/>
        protected CanExecuteDelegate _canExecuteDelegate;
        /// <exclude/>
        protected ExecuteDelegate _executeDelegate;

        //必須要素を与えるコンストラクタ
        /// <summary>
        /// <ja>
        /// コマンドID、カルチャ、説明テキストID、コマンドカテゴリ、コマンドが実行される際に呼び出されるデリゲート、実行可能かどうかを調べる際に呼び出されるデリゲートを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// The object is made specifying delegate called when delegate called when command ID, Culture, explanation text ID, the command category, and the command are executed and it is executable is examined. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="sr">
        /// <ja>
        /// カルチャ情報です。
        /// </ja>
        /// <en>
        /// Information of the culture.
        /// </en>
        /// </param>
        /// <param name="descriptionTextID">
        /// <ja>
        /// コマンドの説明文を示すテキストIDです。
        /// </ja>
        /// <en>
        /// Text ID that shows explanation of command
        /// </en>
        /// </param>
        /// <param name="commandCategory">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en>
        /// </param>
        /// <param name="exec">
        /// <ja>
        /// コマンドが実行されるときに呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed.
        /// </en>
        /// </param>
        /// <param name="canExecute">
        /// <ja>
        /// コマンドが実行可能かどうかを調べる際に呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when it is examined whether command is executable
        /// </en>
        /// </param>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// コマンドオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// Create the command object.
        /// </en>
        /// </summary>
        /// </overloads>
        public GeneralCommandImpl(string commandID, StringResource sr, string descriptionTextID, ICommandCategory commandCategory, ExecuteDelegate exec, CanExecuteDelegate canExecute) {
            _commandID = commandID;
            _usingStringResource = sr != null;
            _strResource = sr;
            _descriptionTextID = descriptionTextID;
            _commandCategory = commandCategory;
            _executeDelegate = exec;
            _canExecuteDelegate = canExecute;
        }
        //一部要素を省略するコンストラクタ群
        /// <summary>
        /// <ja>
        /// コマンドID、カルチャ、説明テキスト文、コマンドカテゴリ、コマンドが実行される際に呼び出されるデリゲート、実行可能かどうかを調べる際に呼び出されるデリゲートを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// The object is made specifying delegate called when delegate called when command ID, Culture, explanation text ID, the command category, and the command are executed and it is executable is examined. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="description">
        /// <ja>
        /// コマンドの説明文を示すテキストです。
        /// </ja>
        /// <en>
        /// Text that shows explanation of command
        /// </en>
        /// </param>
        /// <param name="category">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en>
        /// </param>
        /// <param name="execute">
        /// <ja>
        /// コマンドが実行されるときに呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed.
        /// </en>
        /// </param>
        /// <param name="canExecute">
        /// <ja>
        /// コマンドが実行可能かどうかを調べる際に呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Dalagate called when it is examined whether the command is executable. 
        /// </en>
        /// </param>
        public GeneralCommandImpl(string commandID, string description, ICommandCategory category, ExecuteDelegate execute, CanExecuteDelegate canExecute)
            : this(commandID, null, description, category, execute, canExecute) {
        }
        /// <summary>
        /// <ja>
        /// コマンドID、カルチャ、説明テキストID、コマンドカテゴリ、コマンドが実行される際に呼び出されるデリゲートを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// The object is made specifying delegate called when command ID, culture, explanation text ID, the command category, and the command are executed. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="sr">
        /// <ja>
        /// カルチャ情報です。
        /// </ja>
        /// <en>
        /// Information of the culture.
        /// </en>
        /// </param>
        /// <param name="descriptionTextID">
        /// <ja>
        /// コマンドの説明文を示すテキストIDです。
        /// </ja>
        /// <en>
        /// Text ID that shows explanation of command.
        /// </en>
        /// </param>
        /// <param name="category">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en></param>
        /// <param name="execute">
        /// <ja>
        /// コマンドが実行されるときに呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed
        /// </en>
        /// </param>
        public GeneralCommandImpl(string commandID, StringResource sr, string descriptionTextID, ICommandCategory category, ExecuteDelegate execute)
            : this(commandID, sr, descriptionTextID, category, execute, null) {
        }
        /// <summary>
        /// <ja>
        /// コマンドID、カルチャ、説明テキスト文、コマンドカテゴリ、コマンドが実行される際に呼び出されるデリゲートを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// The object is made specifying delegate called when command ID, culture, explanation text ID, the command category, and the command are executed. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="description">
        /// <ja>
        /// コマンドの説明文を示すテキストです。
        /// </ja>
        /// <en>
        /// Text that shows explanation of command.
        /// </en>
        /// </param>
        /// <param name="category">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en>
        /// </param>
        /// <param name="execute">
        /// <ja>
        /// コマンドが実行されるときに呼び出されるデリゲートです。
        /// </ja>
        /// <en>
        /// Delegate called when command is executed.
        /// </en>
        /// </param>
        public GeneralCommandImpl(string commandID, string description, ICommandCategory category, ExecuteDelegate execute)
            : this(commandID, null, description, category, execute, null) {
        }

        /// <summary>
        /// <ja>
        /// コマンドID、カルチャ、説明テキスト文、コマンドカテゴリを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// The object is made specifying command ID, culture, the explanation text sentence, and the command category. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="sr">
        /// <ja>
        /// カルチャ情報です。
        /// </ja>
        /// <en>
        /// Information of the culture.
        /// </en>
        /// </param>
        /// <param name="descriptionTextID">
        /// <ja>
        /// コマンドの説明文を示すテキストIDです。
        /// </ja>
        /// <en>
        /// Text ID that shows explanation of command.
        /// </en>
        /// </param>
        /// <param name="category">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en>
        /// </param>
        public GeneralCommandImpl(string commandID, StringResource sr, string descriptionTextID, ICommandCategory category)
            : this(commandID, sr, descriptionTextID, category, null, null) {
        }
        /// <summary>
        /// <ja>
        /// コマンドID、説明テキスト文、コマンドカテゴリを指定してオブジェクトを作成します。
        /// </ja>
        /// <en>
        /// Create a object specifying command ID, the explanation text sentence, and the command category. 
        /// </en>
        /// </summary>
        /// <param name="commandID">
        /// <ja>
        /// 割り当てるコマンドIDです。ほかのコマンドとは重複しない唯一無二のものを指定しなければなりません。
        /// </ja>
        /// <en>
        /// It is allocated command ID. The unique one that doesn't overlap should be specified other commands. 
        /// </en>
        /// </param>
        /// <param name="description">
        /// <ja>
        /// コマンドの説明文を示すテキストです。
        /// </ja>
        /// <en>
        /// Text that shows explanation of command
        /// </en>
        /// </param>
        /// <param name="category">
        /// <ja>
        /// コマンドのカテゴリです。
        /// </ja>
        /// <en>
        /// Category of command.
        /// </en>
        /// </param>
        public GeneralCommandImpl(string commandID, string description, ICommandCategory category)
            : this(commandID, null, description, category, null, null) {
        }

        /// <summary>
        /// <ja>
        /// このコマンドに割り当てられているコマンドIDです。
        /// </ja>
        /// <en>
        /// Command ID allocated in this command.
        /// </en>
        /// </summary>
        public virtual string CommandID {
            get {
                return _commandID;
            }
        }

        /// <summary>
        /// <ja>
        /// このコマンドに設定されている説明文です。
        /// </ja>
        /// <en>
        /// Explanation set to this command.
        /// </en>
        /// </summary>
        public virtual string Description {
            get {
                return _usingStringResource ? _strResource.GetString(_descriptionTextID) : _descriptionTextID;
            }
        }

        /// <summary>
        /// <ja>
        /// デフォルトのショートカットキーを示します。
        /// </ja>
        /// <en>
        /// The shortcut key of default is shown. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>デフォルトのショートカットキーが存在しない場合には、Keys.Noneが返されます。</ja>
        /// <en>When the shortcut key of default doesn't exist, Keys.None is returned. </en>
        /// </remarks>
        public virtual Keys DefaultShortcutKey {
            get {
                return _defaultShortcutKey;
            }
        }

        /// <summary>
        /// <ja>
        /// コマンドカテゴリを示します。
        /// </ja>
        /// <en>
        /// The command category is shown. 
        /// </en>
        /// </summary>
        public virtual ICommandCategory CommandCategory {
            get {
                return _commandCategory;
            }
        }

        //Argsが必要なやつは独自に派生する
        /// <summary>
        /// <ja>
        /// オーバーロードです。コマンドが実行されときに呼び出すように設定されたデリゲートを内部で呼び出します。
        /// </ja>
        /// <en>
        /// It is an overload. Delegate set for the command to be executed or to call it is called internally. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>処理対象を示すターゲットです。</ja><en>Target that shows processing object</en></param>
        /// <param name="args"><ja>コマンドに引き渡される任意の引数です。</ja><en>Arbitrary argument handed over to command</en></param>
        /// <returns><ja>実行されたデリゲートの戻り値です。</ja><en>Return value of executed delegate</en></returns>
        public virtual CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            return _executeDelegate == null ? CommandResult.Ignored : _executeDelegate(target);
        }

        /// <summary>
        /// <ja>
        /// オーバーロードです。コマンドが実行可能かどうかの確認呼び出しの際に、設定されたデリゲートを内部で呼び出します。
        /// </ja>
        /// <en>
        /// It is an overload. When it is called whether the command is executable to confirm it, set delegate is called internally. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>処理対象を示すターゲットです。</ja><en>Target that shows processing object.</en></param>
        /// <returns><ja>実行されたデリゲートの戻り値です。</ja><en>Return value of executed delegate.</en></returns>
        public virtual bool CanExecute(ICommandTarget target) {
            return _canExecuteDelegate == null ? true : _canExecuteDelegate(target);
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return CommandManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        /// <summary>
        /// <ja>キーバインドのデフォルト設定を変更します。</ja>
        /// <en>The default setting of key bind is changed. </en>
        /// </summary>
        /// <param name="key"><ja>割り当てたいキー</ja><en>Key that wants to be allocated</en></param>
        /// <returns><ja>このオブジェクト自身を返します。</ja><en>This object is returned. </en></returns>
        public GeneralCommandImpl SetDefaultShortcutKey(Keys key) {
            _defaultShortcutKey = key;
            return this;
        }
    }

    //IPoderosaMenuGroup標準実装
    /// <summary>
    /// <ja>
    /// <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso>を実装したクラスです。メニューグループを作成する際に使います。
    /// </ja>
    /// <en>
    /// It is a class that implements <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso>. When the menu group is made, it uses it. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// メニューを実装する開発者は、このクラスを使うことで、<seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso>
    /// を実装したオブジェクトを容易に作成できます。このクラスは、<seealso cref="IPositionDesignation">IPositionDesignation</seealso>
    /// も実装しており、メニューの順序を決めることもできます。
    /// </para>
    /// <para>
    /// 作成したメニューグループは、拡張ポイントへと登録します。
    /// </para>
    /// <note type="implementnotes">
    /// 一度メニューグループが作成されたなら、それを増減する機能はサポートされていません。またメニュー項目は作成時に決まり、動的に変化することはありません。
    /// </note>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer who implements the menu can easily make the object that implements 
    /// <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso> by using this class. This class implements 
    /// <seealso cref="IPositionDesignation">IPositionDesignation</seealso>, and can decide the order of the menu. 
    /// </para>
    /// <para>
    /// The menu group that makes it registers to the extension point. 
    /// </para>
    /// <note type="implementnotes">
    /// If the menu group was made once, the function to increase and decrease it is not supported. Moreover, the menu item is decided when making it, and never changes dynamically. 
    /// </note>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso>を実装したオブジェクトは、次のようにして作成できます。
    /// <code>
    /// // あらかじめメニューが実行されたときのメニューとメニュー項目を作成しておきます。
    /// 
    /// // コマンド
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行されたときの処理
    ///    MessageBox.Show("実行されました");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行できるかどうかを返す
    ///    return true;
    ///  }
    /// );
    /// 
    /// // メニュー項目
    /// PoderosaMenuItemImpl menuitem = new PoderosaMenuItemImpl(
    ///     mycommand, "My Menu Name");
    ///
    /// // メニューグループ
    /// PoderosaMenuGroupImpl menugroup = new PoderosaMenuGroupImpl(menuitem);
    /// 
    /// // このメニューグループを、たとえば［編集］メニュー（org.poderosa.menu.edit）に登録
    /// // 拡張ポイントを検索
    /// IExtensionPoint editmenu = 
    ///     PoderosaWorld.PluginManager.FindExtensionPoint("org.poderosa.menu.edit");
    /// // 拡張ポイントにメニューグループを登録
    /// editmenu.RegisterExtension(menugroup);
    /// </code>
    /// </ja>
    /// <en>
    /// The object that implements <seealso cref="IPoderosaMenuGroup">IPoderosaMenuGroup</seealso> can be made as follows. 
    /// <code>
    /// // The menu and the menu item when the menu is executed are made beforehand. 
    /// 
    /// // Command
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // Processing when command is executed
    ///    MessageBox.Show("Executed");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // It is returned whether the command can be executed. 
    ///    return true;
    ///  }
    /// );
    /// 
    /// // Menu item.
    /// PoderosaMenuItemImpl menuitem = new PoderosaMenuItemImpl(
    ///     mycommand, "My Menu Name");
    ///
    /// // Menu group.
    /// PoderosaMenuGroupImpl menugroup = new PoderosaMenuGroupImpl(menuitem);
    /// 
    /// // For instance, this menu group is registered in "Edit" menu (org.poderosa.menu.edit). 
    /// // Retrieval of The enhancing point.
    /// IExtensionPoint editmenu = 
    ///     PoderosaWorld.PluginManager.FindExtensionPoint("org.poderosa.menu.edit");
    /// // The menu group is registered in the extension point. 
    /// editmenu.RegisterExtension(menugroup);
    /// </code>
    /// </en>
    /// </example>
    public class PoderosaMenuGroupImpl : IPoderosaMenuGroup, IPositionDesignation {
        /// <exclude/>
        protected IPoderosaMenu[] _childMenus;
        /// <exclude/>
        protected bool _isVolatile;
        /// <exclude/>
        protected bool _showSeparator;
        /// <exclude/>
        protected IAdaptable _designationTarget;
        /// <exclude/>
        protected PositionType _positionType;

        /// <summary>
        /// <ja>
        /// 含まれるメニュー項目がひとつもないメニューグループを作成します。
        /// </ja>
        /// <en>
        /// The included menu item makes the menu group that is not no. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// メニュー項目はひとつもありませんが、区切り記号（セパレータ）は表示されます。
        /// </ja>
        /// <en>
        /// The separator is displayed though nothing is in the menu item. 
        /// </en>
        /// </remarks>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// メニューグループを作成します。
        /// </ja>
        /// <en>
        /// Create the menu group.
        /// </en>
        /// </summary>
        /// </overloads>
        public PoderosaMenuGroupImpl()
            : this(null, true) {
        }

        /// <summary>
        /// <ja>
        /// 含まれるメニュー項目をひとつだけ指定したメニューグループを作成します。
        /// </ja>
        /// <en>
        /// The menu group that specifies only one included menu item is made. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 区切り記号（セパレータ）は表示されます。
        /// </ja>
        /// <en>
        /// The separator is displayed. 
        /// </en>
        /// </remarks>
        /// <param name="child"><ja>含めたいメニュー項目です。</ja><en>Menu item that wants to be included</en></param>
        public PoderosaMenuGroupImpl(IPoderosaMenu child)
            : this(new IPoderosaMenu[] { child }, true) {
        }

        /// <summary>
        /// <ja>
        /// 含まれるメニュー項目を複数指定したメニューグループを作成します。
        /// </ja>
        /// <en>
        /// The menu group that specifies two or more included menu items is made. 
        /// </en>
        /// </summary>
        /// <param name="childMenus"><ja>含めたいメニュー項目の配列です。</ja><en>Array of menu item that wants to be included</en></param>
        /// <remarks>
        /// <ja>
        /// 区切り記号（セパレータ）は表示されます。
        /// </ja>
        /// <en>
        /// The separator is displayed. 
        /// </en>
        /// </remarks>
        public PoderosaMenuGroupImpl(IPoderosaMenu[] childMenus)
            :
            this(childMenus, true) {
        }
        /// <summary>
        /// <ja>
        /// 含まれる複数のメニュー項目とメニュー項目の直前に区切り記号（セパレータ）を表示するか否か
        /// を指定してメニューグループを作成します。
        /// </ja>
        /// <en>
        /// The menu group is made specifying whether to display the separator just before two or more included menu item and menu item. 
        /// </en>
        /// </summary>
        /// <param name="childMenus"><ja>含めたいメニュー項目の配列です。</ja><en>Array of menu item that wants to be included</en></param>
        /// <param name="showSeparator"><ja>セパレータを表示するか否かの指定です。trueのとき表示、falseのとき非表示です。</ja>
        /// <en>It is specification whether to display the separator. It displays at true, and non-display at false. </en></param>
        public PoderosaMenuGroupImpl(IPoderosaMenu[] childMenus, bool showSeparator) {
            _childMenus = childMenus;
            _isVolatile = false;
            _showSeparator = showSeparator;
            _designationTarget = null;
            _positionType = PositionType.First;
        }

        /// <summary>
        /// <ja>
        /// 含まれるメニュー項目の配列です。
        /// </ja>
        /// <en>
        /// Array of included menu item
        /// </en>
        /// </summary>
        public virtual IPoderosaMenu[] ChildMenus {
            get {
                return _childMenus;
            }
        }

        /// <summary>
        /// <ja>
        /// メニューが動的に作成されるかどうかを示すプロパティです。
        /// </ja>
        /// <en>
        /// Property that shows whether menu is dynamically made.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>常にfalse（メニューを動的に作成しない）が返されます。</ja>
        /// <en>False (The menu is not dynamically made) is always returned. </en>
        /// </remarks>
        public virtual bool IsVolatileContent {
            get {
                return _isVolatile;
            }
        }

        /// <summary>
        /// <ja>
        /// 区切り記号（セパレータ）を表示するか否かを示します。
        /// </ja>
        /// <en>
        /// It is shown whether to display the separator. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// trueの場合、メニューグループの直前に区切り記号（セパレータ）が表示されます。falseの場合には表示されません。
        /// </ja>
        /// <en>
        /// The separator is displayed for true just before the menu group. It is not displayed for false. 
        /// </en>
        /// </remarks>
        public virtual bool ShowSeparator {
            get {
                return _showSeparator;
            }
        }


        /// <summary>
        /// <ja>
        /// メニューを配置する場所の対象を示します。
        /// </ja>
        /// <en>
        /// The object of the place where the menu is arranged is shown. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPositionDesignation">IPositionDesignation</seealso>を参照してください。
        /// </ja>
        /// <en>
        /// Please refer to <seealso cref="IPositionDesignation">IPositionDesignation</seealso>.
        /// </en>
        /// </remarks>
        public virtual IAdaptable DesignationTarget {
            get {
                return _designationTarget;
            }
        }

        /// <summary>
        /// <ja>
        /// メニューの表示位置を示します。
        /// </ja>
        /// <en>
        /// The position where the menu is displayed is shown. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <seealso cref="IPositionDesignation">IPositionDesignation</seealso>を参照してください。
        /// </ja>
        /// <en>
        /// Refer to <seealso cref="IPositionDesignation">IPositionDesignation</seealso>.
        /// </en>
        /// </remarks>
        public virtual PositionType DesignationPosition {
            get {
                return _positionType;
            }
        }
        public virtual IAdaptable GetAdapter(Type adapter) {
            return CommandManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        //ポジションセット
        /// <summary>
        /// <ja>
        /// メニューの位置を設定します。
        /// </ja>
        /// <en>
        /// Set The position of the menu.
        /// </en>
        /// </summary>
        /// <param name="type">
        /// <ja>メニューの場所を指定します。</ja>
        /// <en>Specifies the place of the menu.</en>
        /// </param>
        /// <param name="target">
        /// <ja>どのメニューに対する位置なのかを指定します。<see cref="IPoderosaMenuGroup">IPoderosaMenuGroup</see>でなければなりません。</ja>
        /// <en>To which menu position it is is specified. It should be <see cref="IPoderosaMenuGroup">IPoderosaMenuGroup</see>. </en>
        /// </param>
        /// <returns><ja>このオブジェクト自身が戻ります。</ja><en>This object returns. </en></returns>
        /// <remarks>
        /// <ja>
        /// デフォルトでは、メニュー位置は、「先頭（PositionType.First）」に設定されます。詳細は、<seealso cref="IPositionDesignation">IPositionDesignation</seealso>を参照してください。
        /// </ja>
        /// <en>
        /// In default, the menu position is set to the "head(PositionType.First)". 
        /// Refer to <seealso cref="IPositionDesignation">IPositionDesignation</seealso> for more information.
        /// </en>
        /// </remarks>
        public PoderosaMenuGroupImpl SetPosition(PositionType type, IAdaptable target) {
            _positionType = type;
            _designationTarget = target;
            return this;
        }
    }


    //IPoderosaMenuItem標準実装
    /// <summary>
    /// <ja>
    /// <seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso>を実装したクラスです。引数なしで実行されるコマンドを定義するメニュー項目を作成する際に使います。
    /// </ja>
    /// <en>
    /// It is a class that implements <seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso>. When the menu item that defines the command executed without the argument is made, it uses it. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// メニュー項目を実装する開発者は、このクラスを使うことで<seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso>を実装したオブジェクト
    /// を容易に作成できます。
    /// </ja>
    /// <en>
    /// The developer who implements the menu item can easily make the object where <seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso> is implemented by using this class. 
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// <seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso>を実装したオブジェクトは、次のようにして作成できます。
    /// <code>
    /// // あらかじめメニューが実行されたときのメニューとメニュー項目を作成しておきます。
    /// 
    /// // コマンド
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行されたときの処理
    ///    MessageBox.Show("実行されました");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // コマンドが実行できるかどうかを返す
    ///    return true;
    ///  }
    /// );
    /// 
    /// // メニュー項目を作成
    /// PoderosaMenuItemImpl menuitem = new PoderosaMenuItemImpl(
    ///     mycommand, "My Menu Name");
    /// </code>
    /// </ja>
    /// <en>
    /// The object that implements <seealso cref="IPoderosaMenuItem">IPoderosaMenuItem</seealso> can be made as follows. 
    /// <code>
    /// // The menu and the menu item when the menu is executed are made beforehand. 
    /// 
    /// // Command.
    /// PoderosaCommandImpl mycommand = new PoderosaCommandImpl(
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // Processing when command is executed
    ///    MessageBox.Show("Executed");
    ///    return CommandResult.Succeeded;
    ///   },
    ///   delegate(ICommandTarget target)
    ///   {
    ///     // Returned whether the command can be executed. 
    ///    return true;
    ///  }
    /// );
    /// 
    /// // Create the menu item.
    /// PoderosaMenuItemImpl menuitem = new PoderosaMenuItemImpl(
    ///     mycommand, "My Menu Name");
    /// </code>
    /// </en>
    /// </example>
    public class PoderosaMenuItemImpl : IPoderosaMenuItem {
        /// <exclude/>
        protected IPoderosaCommand _command;
        /// <exclude/>
        protected bool _usingStringResource;
        /// <exclude/>
        protected StringResource _strResource;
        /// <exclude/>
        protected string _textID;
        /// <exclude/>
        protected CheckedDelegate _checked;

        /// <summary>
        /// <ja>
        /// コマンドIDとメニューの表示名を指定してメニュー項目を作成します。
        /// </ja>
        /// <en>
        /// The menu item is made specifying the display name of command ID and the menu. 
        /// </en>
        /// </summary>
        /// <param name="command_id"><ja>メニューが選択されたときに呼び出したいコマンドIDです。</ja><en>It is command ID that wants to call when the menu is selected. </en></param>
        /// <param name="text"><ja>メニューに表示するテキストです。</ja><en>Text displayed in menu.</en></param>
        /// <remarks>
        /// <ja>
        /// <paramref name="command_id">command_id</paramref>に指定したコマンドIDが見つからないときには、<see cref="P:Poderosa.Commands.PoderosaMenuItemImpl.AssociatedCommand">AssociatedCommandプロパティ</see>
        /// がnullになります。
        /// </ja>
        /// <en>
        /// When command ID specified for <paramref name="command_id">command_id</paramref> is not found, the <see cref="AssociatedCommand">AssociatedCommand property</see> becomes null. 
        /// </en>
        /// </remarks>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// メニュー項目を作成します。
        /// </ja>
        /// <en>
        /// Create the menu item.
        /// </en>
        /// </summary>
        /// </overloads>
        public PoderosaMenuItemImpl(string command_id, string text)
            : this(BindCommand(command_id), null, text) {
        }
        /// <summary>
        /// <ja>
        /// 実行するコマンドの<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>とメニューの表示名を指定してメニュー項目を作成します。
        /// </ja>
        /// <en>
        /// The menu item is made specifying the display name of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> and the menu of the executed command. 
        /// </en>
        /// </summary>
        /// <param name="command"><ja>メニューが選択されたときに呼び出したいコマンドです。</ja><en>It is command that wants to call when the menu is selected. </en></param>
        /// <param name="text"><ja>メニューに表示するテキストです。</ja><en>Text displayed in menu.</en></param>
        /// <remarks>
        /// <ja><paramref name="command">command</paramref>にnullを指定してはいけません。</ja><en>Do not specify null for command. </en>
        /// </remarks>
        public PoderosaMenuItemImpl(IPoderosaCommand command, string text)
            : this(command, null, text) {
        }

        /// <summary>
        /// <ja>
        /// コマンドIDとカルチャ、メニューの表示名を指定してメニュー項目を作成します。
        /// </ja>
        /// <en>
        /// The menu item is made specifying the display name of command ID, culture, and the menu. 
        /// </en>
        /// </summary>
        /// <param name="command_id"><ja>メニューが選択されたときに呼び出したいコマンドIDです。</ja><en>Command ID that wants to call when menu is selected</en></param>
        /// <param name="sr"><ja>カルチャ情報です。</ja><en>Information of culture.</en></param>
        /// <param name="textID"><ja>メニューに表示するテキストIDです。</ja><en>Text ID displayed in menu</en></param>
        /// <remarks>
        /// <ja>
        /// <paramref name="command_id">command_id</paramref>に指定したコマンドIDが見つからないときには、<see cref="AssociatedCommand">AssociatedCommandプロパティ</see>
        /// がnullになります。
        /// </ja>
        /// <en>
        /// When command ID specified for <paramref name="command_id">command_id</paramref> is not found, the <see cref="AssociatedCommand">AssociatedCommand property</see> becomes null. 
        /// </en>
        /// </remarks>
        public PoderosaMenuItemImpl(string command_id, StringResource sr, string textID)
            : this(BindCommand(command_id), sr, textID) {
        }

        /// <summary>
        /// <ja>実行するコマンドの<seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>、カルチャ、メニューの表示名を指定してメニュー項目を作成します。</ja>
        /// <en>The menu item is made specifying the display name of executed <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> of the command, culture, and menu. </en>
        /// </summary>
        /// <param name="command"><ja>メニューが選択されたときに呼び出したいコマンドです。</ja><en>Command that wants to call when menu is selected</en></param>
        /// <param name="sr"><ja>カルチャ情報です。</ja><en>Information of culture.</en></param>
        /// <param name="textID"><ja>メニューに表示するテキストIDです。</ja><en>Text ID displayed in menu</en></param>
        public PoderosaMenuItemImpl(IPoderosaCommand command, StringResource sr, string textID) {
            Debug.Assert(command != null);
            _command = command;
            _usingStringResource = sr != null;
            _strResource = sr;
            _textID = textID;
        }
        private static IGeneralCommand BindCommand(string command_id) {
            IGeneralCommand cmd = CommandManagerPlugin.Instance.Find(command_id);
            Debug.Assert(cmd != null, "Command Not Found");
            return cmd;
        }

        /// <summary>
        /// <ja>
        /// メニューが選択されたときに呼び出されるコマンドを示します。
        /// </ja>
        /// <en>
        /// The command called when the menu is selected is shown. 
        /// </en>
        /// </summary>
        public virtual IPoderosaCommand AssociatedCommand {
            get {
                return _command;
            }
        }

        /// <summary>
        /// <ja>
        /// メニューに表示するテキストを示します。
        /// </ja>
        /// <en>
        /// The text displayed in the menu is shown. 
        /// </en>
        /// </summary>
        public virtual string Text {
            get {
                return _usingStringResource ? _strResource.GetString(_textID) : _textID;
            }
        }

        /// <summary>
        /// <ja>
        /// メニューが選択可能かどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the menu can be selected. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>処理対象となるターゲットです。</ja><en>Target to be processed</en></param>
        /// <returns><ja>選択可能ならtrue、選択不可ならfalseが返されます。</ja><en>If it is selectable, return true. It isn't, return false.</en></returns>
        /// <remarks>
        /// <ja>
        /// このメソッドは、内部で<see cref="AssociatedCommand">AssiciatedCommand</see>プロパティで示された
        /// <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso>の<seealso cref="IPoderosaCommand.CanExecute">CanExecuteメソッド</seealso>
        /// を呼び出すことで実装されています。
        /// </ja>
        /// <en>
        /// This method is implemented by calling the <seealso cref="IPoderosaCommand.CanExecute">CanExecute method</seealso> of <seealso cref="IPoderosaCommand">IPoderosaCommand</seealso> shown internally in the <see cref="AssociatedCommand">AssiciatedCommand</see> property. 
        /// </en>
        /// </remarks>
        public virtual bool IsEnabled(ICommandTarget target) {
            return _command.CanExecute(target);
        }

        /// <summary>
        /// <ja>
        /// メニューにチェックが付けられているかどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the menu is checked. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>処理対象となるターゲットです。</ja><en>Target to be processed.</en></param>
        /// <returns><ja>チェックが付いているならtrue、そうでないならfalseが返されます。</ja><en>It is true, and a return of false if checked if not so. </en></returns>
        /// <remarks>
        /// <ja>
        /// このメソッドは、常にfalseが戻ります。
        /// </ja>
        /// <en>
        /// False always returns in this method. 
        /// </en>
        /// </remarks>
        public virtual bool IsChecked(ICommandTarget target) {
            return _checked == null ? false : _checked(target);
        }

        public virtual IAdaptable GetAdapter(Type adapter) {
            return CommandManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
