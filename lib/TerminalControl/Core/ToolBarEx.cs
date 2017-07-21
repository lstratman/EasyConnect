/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ToolBarEx.cs,v 1.3 2012/03/11 12:19:21 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Poderosa.Commands;

namespace Poderosa.Forms {
    /// <summary>
    /// <ja>
    /// ツールバーに含まれる要素を示す基底インターフェイスです。
    /// </ja>
    /// <en>
    /// Base interface that shows element included in toolbar.
    /// </en>
    /// </summary>
    public interface IToolBarElement : IAdaptable {
        /// <summary>
        /// <ja>
        /// 要素のツールチップテキストを示します。
        /// </ja>
        /// <en>
        /// The tooltip text of the element is shown. 
        /// </en>
        /// </summary>
        string ToolTipText {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー内のラベルを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows label in toolbar.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者がラベルを作る場合、このインターフェイスを実装したオブジェクトを作る代わりに、<seealso cref="ToolBarLabelImpl">ToolBarLabelImpl</seealso>を使うことができます。
    /// </ja>
    /// <en>
    /// <seealso cref="ToolBarLabelImpl">ToolBarLabelImpl</seealso> can be used instead of making the object that mounts this interface when the developer creates labels. 
    /// </en>
    /// </remarks>
    public interface IToolBarLabel : IToolBarElement {
        /// <summary>
        /// <ja>ラベルのテキストです。</ja>
        /// <en>Text of the label.</en>
        /// </summary>
        string Text {
            get;
        }
        /// <summary>
        /// <ja>ラベルの幅です。単位はピクセルです。</ja>
        /// <en>Width of the label. The unit is a pixel. </en>
        /// </summary>
        int Width {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー内のボタンを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows button in toolbar.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者がボタンを作る場合、このインターフェイスを実装したオブジェクトを作る代わりに、<seealso cref="ToolBarCommandButtonImpl">ToolBarCommandButtonImpl</seealso>を使うことができます。
    /// </ja>
    /// <en>
    /// <seealso cref="ToolBarCommandButtonImpl">ToolBarCommandButtonImpl</seealso> can be used instead of making the object that implements this interface when the developer makes the button. 
    /// </en>
    /// </remarks>
    public interface IToolBarCommandButton : IToolBarElement {
        /// <summary>
        /// <ja>
        /// ボタンがクリックされたときに実行されるコマンドです。
        /// </ja>
        /// <en>
        /// It is a command executed when the button is clicked. 
        /// </en>
        /// </summary>
        IPoderosaCommand Command {
            get;
        }
        /// <summary>
        /// <ja>
        /// ボタン表面に表示するアイコンです。
        /// </ja>
        /// <en>
        /// Icon displayed on the surface of the button. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// アイコンの大きさは16×16ピクセルでなければなりません。
        /// </ja>
        /// <en>
        /// The size of the icon should be 16×16 pixels. 
        /// </en>
        /// </remarks>
        Image Icon {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー内のコンボボックスを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows combobox in toolbar.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者がコンボボックスを作る場合、このインターフェイスを実装したオブジェクトを作る代わりに、<seealso cref="ToolBarComboBoxImpl">ToolBarComboBoxImpl</seealso>を使うことができます。
    /// </ja>
    /// <en>
    /// <seealso cref="ToolBarComboBoxImpl">ToolBarComboBoxImpl</seealso> can be used instead of making the object that implements this interface when the developer makes the combo box. 
    /// </en>
    /// </remarks>
    public interface IToolBarComboBox : IToolBarElement {
        /// <summary>
        /// <ja>
        /// コンボボックス内に表示する選択肢となるアイテムです。
        /// </ja>
        /// <en>
        /// Item that becomes choices displayed in combo box.
        /// </en>
        /// </summary>
        object[] Items {
            get;
        }
        /// <summary>
        /// <ja>
        /// コンボボックスの幅です。単位はピクセルです。
        /// </ja>
        /// <en>Width of the combo box. The unit is a pixel. </en>
        /// </summary>
        int Width {
            get;
        }
        /// <summary>
        /// <ja>このコンボボックスが選択できるかどうかを示します。</ja>
        /// <en>It is shown whether this combo box can be selected. </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>target for execution. </en></param>
        /// <returns><ja>選択できるときにはtrue、そうでないときにはfalseが返されます。</ja><en>False is returned when it is not true so when it is possible to select it. </en></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is a toolbar that belongs to <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>.
        /// </en>
        /// </remarks>
        bool IsEnabled(ICommandTarget target);
        /// <summary>
        /// <ja>
        /// 現在選択されているアイテムのインデックス番号を返します。
        /// </ja>
        /// <en>
        /// Return the index of the item that has been selected now.
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja><paramref name="target">target</paramref>のインデックス位置が返されます。</ja>
        /// <en>Return index position of the <paramref name="target">target</paramref></en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is a toolbar that belongs to <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>.
        /// </en>
        /// </remarks>
        int GetSelectedIndex(ICommandTarget target);
        /// <summary>
        /// <ja>
        /// コンボボックスで選択されている選択肢が変化したときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of call when choices that have been selected in combobox change
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution.</en></param>
        /// <param name="selectedIndex"><ja>ユーザーが選択したアイテムのインデックス番号です。</ja><en>Index of item that user selected.</en></param>
        /// <param name="selectedItem"><ja>ユーザーが選択したアイテムのオブジェクトです。</ja><en>An object of item that user selected.</en></param>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        void OnChange(ICommandTarget target, int selectedIndex, object selectedItem);
    }

    /// <summary>
    /// <ja>
    /// ツールバー内のトグルボタンを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows Toggle button in toolbar.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者がトグルボタンを作る場合、このインターフェイスを実装したオブジェクトを作る代わりに、<seealso cref="ToolBarToggleButtonImpl">ToolBarToggleButtonImpl</seealso>を使うことができます。
    /// </ja>
    /// <en>
    /// <seealso cref="ToolBarToggleButtonImpl">ToolBarToggleButtonImpl</seealso> can be used instead of making the object that implements this interface when the developer makes the toggle button. 
    /// </en>
    /// </remarks>
    public interface IToolBarToggleButton : IToolBarElement {
        /// <summary>
        /// <ja>
        /// ボタン表面に表示するアイコンです。
        /// </ja>
        /// <en>
        /// Icon displayed on surface of button
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// アイコンの大きさは16×16ピクセルでなければなりません。
        /// </ja>
        /// <en>
        /// The size of the icon should be 16×16 pixels. 
        /// </en>
        /// </remarks>
        Image Icon {
            get;
        }
        /// <summary>
        /// <ja>このトグルボタンが選択できるかどうかを示します。</ja>
        /// <en>It is shown whether this toggle button can be selected. </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja>選択できるときにはtrue、そうでないときにはfalseが返されます。</ja><en>False is returned when it is not true so when it is possible to select it. </en></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        bool IsEnabled(ICommandTarget target);
        /// <summary>
        /// <ja>このトグルボタンのオン／オフの状態を示します。</ja>
        /// <en>The state of on/off of this toggle button is shown. </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja>
        /// <en>Target for execution</en>
        /// </param>
        /// <returns><ja>オンのとき（凹んでいる状態）のときにはtrue、オフであるとき（凹んでいない状態）のときにはfalseが返されます。</ja>
        /// <en>False is returned when it is true off (state that doesn't dent) when turning it on (state that has dented). </en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        bool IsChecked(ICommandTarget target);
        /// <summary>
        /// <ja>
        /// トグルボタンのオン／オフの状態が変わったときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// Method of the call when the state of on/off of the toggle button changes. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <param name="is_checked"><ja>オンにされたときにはtrue、オフにされたときにはfalseです。</ja>
        /// <en>When turned off true, it is false when turned on. </en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is a toolbar that belongs to <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>.
        /// </en>
        /// </remarks>
        void OnChange(ICommandTarget target, bool is_checked);
    }

    /// <summary>
    /// <ja>
    /// ツールバーを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows toolbar.
    /// </en>
    /// </summary>
    public interface IToolBar : IAdaptable {
        /// <summary>
        /// <ja>ツールバーが属するウィンドウです。</ja>
        /// <en>Window to which toolbar belongs</en>
        /// </summary>
        IPoderosaMainWindow ParentWindow {
            get;
        }
        /// <summary>
        /// <ja>すべての要素を再描画します。</ja>
        /// <en>It draws in all elements again. </en>
        /// </summary>
        void RefreshAll();
        /// <summary>
        /// <ja>指定したコンポーネントを再描画します。</ja>
        /// <en>It draws in the specified component again. </en>
        /// </summary>
        /// <param name="component"><ja>再描画したいコンポーネント</ja><en>Component where it wants to draw again</en></param>
        void RefreshComponent(IToolBarComponent component);
        /// <summary>
        /// <ja>
        /// ツールバーの位置を文字列として構成したものを返します。
        /// </ja>
        /// <en>
        /// Return the one that the position of the toolbar was composed as a character string.
        /// </en>
        /// </summary>
        /// <returns><ja>ツールバーの位置を書式化した文字列</ja><en>Character string that makes position of toolbar format</en></returns>
        /// <remarks>
        /// <ja>
        /// このメソッドから戻された文字列は、Preferenceにツールバー位置を保存するときに使われます。
        /// </ja>
        /// <en>
        /// When the toolbar position is preserved in Preference, the character string returned from this method is used. 
        /// </en>
        /// </remarks>
        string FormatLocations();
    }

    /// <summary>
    /// <ja>
    /// ツールバーコンポーネントを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows toolbar component.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ツールバー内のラベル、ボタン、トグルボタン、コンボボックスは、<see cref="ToolBarElements">ToolBarElemnentsプロパティ</see>に配列として格納します。
    /// </ja>
    /// <en>
    /// The label, the button, the toggle button, and the combobox in the toolbar are stored in the <see cref="ToolBarElements">ToolBarElemnents property</see> as an array. 
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// ツールバーコンポーネントを作る例を示します。
    /// <code>
    /// [assembly: PluginDeclaration(typeof(MyPlugin.HelloWorldPlugin))]
    /// namespace MyPlugin
    /// {
    ///    [PluginInfo(ID="jp.example.helloworld", Version="1.0",
    ///        Dependencies="org.poderosa.core.window")]
    ///
    ///    // ここではプラグイン自身にIToolBarComponentを実装
    ///    internal class HelloWorldPlugin : PluginBase, IToolBarComponent
    ///    {
    ///        private IToolBarElement[] _elements;
    ///
    ///        public override void InitializePlugin(IPoderosaWorld poderosa)
    ///        {
    ///            base.InitializePlugin(poderosa);
    ///            
    ///            // （1）コマンドオブジェクトを用意する
    ///            PoderosaCommandImpl btncommand = new PoderosaCommandImpl(
    ///              delegate(ICommandTarget target)
    ///              {
    ///                  // 実行されたときのコマンド
    ///                  MessageBox.Show("ボタンがクリックされました");
    ///                  return CommandResult.Succeeded;
    ///              },
    ///              delegate(ICommandTarget target)
    ///              {
    ///                  // コマンドが実行できるかどうかを示すデリゲート
    ///                  return true;
    ///              }
    ///            );
    ///
    ///            // （2）要素としてボタンを作る（myImageは16×16のビットマップ）
    ///            System.Drawing.Image myImage = 
    ///              new System.Drawing.Bitmap("画像ファイル名");
    ///            ToolBarCommandButtonImpl btn = ]
    ///              new ToolBarCommandButtonImpl(btncommand, myImage);
    ///
    ///            // 要素として設定
    ///            _elements = new IToolBarElement[]{ btn };
    ///
    ///            // （3）拡張ポイントを検索して登録
    ///            IExtensionPoint toolbarExt = 
    ///              poderosa.PluginManager.FindExtensionPoint("org.poderosa.core.window.toolbar");
    ///            // 登録
    ///            toolbarExt.RegisterExtension(this);
    ///        }
    ///
    ///        public IToolBarElement[] ToolBarElements
    ///        {
    ///            // 要素を返す
    ///            get { return _elements; }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </ja>
    /// <en>
    /// The example of making the toolbar component is shown. 
    /// <code>
    /// [assembly: PluginDeclaration(typeof(MyPlugin.HelloWorldPlugin))]
    /// namespace MyPlugin
    /// {
    ///    [PluginInfo(ID="jp.example.helloworld", Version="1.0",
    ///        Dependencies="org.poderosa.core.window")]
    ///
    ///    // Here, implmenent IToolBarComponent to this plug-in.
    ///    internal class HelloWorldPlugin : PluginBase, IToolBarComponent
    ///    {
    ///        private IToolBarElement[] _elements;
    ///
    ///        public override void InitializePlugin(IPoderosaWorld poderosa)
    ///        {
    ///            base.InitializePlugin(poderosa);
    ///            
    ///            // (1) Prepare the command object.
    ///            PoderosaCommandImpl btncommand = new PoderosaCommandImpl(
    ///              delegate(ICommandTarget target)
    ///              {
    ///                  // Command when executed.
    ///                  MessageBox.Show("Button is clicked.");
    ///                  return CommandResult.Succeeded;
    ///              },
    ///              delegate(ICommandTarget target)
    ///              {
    ///                  // Delegate that shows whether command can be executed
    ///                  return true;
    ///              }
    ///            );
    ///
    ///            // (2)Create the button as element (myImage is a bitmap that size is 16x16)
    ///            System.Drawing.Image myImage = 
    ///              new System.Drawing.Bitmap("Graphics file name.");
    ///            ToolBarCommandButtonImpl btn = ]
    ///              new ToolBarCommandButtonImpl(btncommand, myImage);
    ///
    ///            // Set as element.
    ///            _elements = new IToolBarElement[]{ btn };
    ///
    ///            // (3)Retrieve the extension point and regist.
    ///            IExtensionPoint toolbarExt = 
    ///              poderosa.PluginManager.FindExtensionPoint("org.poderosa.core.window.toolbar");
    ///            // Regist
    ///            toolbarExt.RegisterExtension(this);
    ///        }
    ///
    ///        public IToolBarElement[] ToolBarElements
    ///        {
    ///            // Return the element.
    ///            get { return _elements; }
    ///        }
    ///    }
    ///}
    /// </code>
    /// </en>
    /// </example>
    public interface IToolBarComponent : IAdaptable {
        /// <summary>
        /// <ja>
        /// ツールバーコンポーネントに含まれる、ラベル、ボタン、トグルボタン、コンボボックスの配列です。
        /// </ja>
        /// <en>
        /// Inclusion in toolbar component, and array of label, button, toggle button, and combobox
        /// </en>
        /// </summary>
        IToolBarElement[] ToolBarElements {
            get;
        }
    }

    //各ToolBarElementの標準実装
    /// <summary>
    /// <ja>
    /// ツールバーの要素の基底となるクラスです。
    /// </ja>
    /// <en>
    /// Class that becomes base of element of toolbar.
    /// </en>
    /// </summary>
    public abstract class ToolBarElementImpl : IToolBarElement {
        public virtual IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        /// <summary>
        /// <ja>
        /// ツールチップテキストを返します。
        /// </ja>
        /// <en>
        /// Return the tooltip text.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// デフォルトでは、から文字（""）を返すように実装されています。必要に応じてオーバーライドしてください。
        /// </ja>
        /// <en>
        /// In default, to return the null character (""), it is implemente. Please do override if necessary. 
        /// </en>
        /// </remarks>
        public virtual string ToolTipText {
            get {
                return "";
            }
        }
    }
    /// <summary>
    /// <ja>
    /// ツールバー要素のラベルを構成する機能を提供します。
    /// </ja>
    /// <en>
    /// The function to compose the label of the toolbar element is offered. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者はこのクラスを用いることで、<seealso cref="IToolBarLabel">IToolBarLabel</seealso>
    /// を備えたオブジェクトを構成できます。
    /// </ja>
    /// <en>
    /// The developer can compose the object that has <seealso cref="IToolBarLabel">IToolBarLabel</seealso> by using this class. 
    /// </en>
    /// </remarks>
    public class ToolBarLabelImpl : ToolBarElementImpl, IToolBarLabel {
        /// <summary>
        /// <ja>
        /// カルチャ情報を示す内部変数です。
        /// </ja>
        /// <en>
        /// Internal variable that shows culture information
        /// </en>
        /// </summary>
        protected StringResource _res;
        /// <summary>
        /// <ja>
        /// カルチャ情報を使うかどうかを示す内部変数です。
        /// </ja>
        /// <en>
        /// It is an internal variable that shows whether to use culture information. 
        /// </en>
        /// </summary>
        protected bool _usingStringResource;
        /// <summary>
        /// <ja>
        /// ラベル幅を示す内部変数です。単位はピクセルです。
        /// </ja>
        /// <en>
        /// It is an internal variable that shows the width of the label. The unit is a pixel. 
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このプロパティの値は<seealso cref="Width">Widthプロパティ</seealso>から返されます。
        /// </ja>
        /// <en>
        /// The value of this property is returned by the Width property. 
        /// </en>
        /// </remarks>
        protected int _width;
        /// <summary>
        /// <ja>
        /// ラベルとして表示するテキストを保持する内部変数です。
        /// </ja>
        /// <en>
        /// It is an internal variable that holds the text to show as the label.
        /// </en>
        /// </summary>
        protected string _text;

        /// <summary>
        /// <ja>
        /// 空のラベルを作成します。
        /// </ja>
        /// <en>
        /// Create a null label.
        /// </en>
        /// </summary>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// ツールバーのラベルを作成します。
        /// </ja>
        /// <en>
        /// Create a label of toolbar.
        /// </en>
        /// </summary>
        /// </overloads>
        public ToolBarLabelImpl() {
        }

        /// <summary>
        /// <ja>カルチャ情報を指定してラベルを作成します。</ja>
        /// <en>Create label specified with culture information</en>
        /// </summary>
        /// <param name="res"><ja>カルチャ情報です</ja><en>Culture information.</en></param>
        /// <param name="text"><ja>ラベルに表示するテキストIDです。</ja><en>The text ID to show on the label.</en></param>
        /// <param name="width"><ja>ラベルの幅です。単位はピクセルです。</ja><en>WIdth of the label. The unit is a pixel.</en></param>

        public ToolBarLabelImpl(StringResource res, string text, int width) {
            _res = res;
            _usingStringResource = true;
            _text = text;
            _width = width;
        }

        /// <summary>
        /// <ja>
        /// テキストと幅を指定してラベルを作成します。
        /// </ja>
        /// <en>
        /// Create the label specifying the text and width. 
        /// </en>
        /// </summary>
        /// <param name="text"><ja>ラベルに表示するテキストです。</ja><en>The text to show on the label.</en></param>
        /// <param name="width"><ja>ラベルの幅です。単位はピクセルです。</ja><en>WIdth of the label. The unit is a pixel.</en></param>
        public ToolBarLabelImpl(string text, int width) {
            _usingStringResource = false;
            _text = text;
            _width = width;
        }

        /// <summary>
        /// <ja>
        /// ラベルに表示するテキストを返します。
        /// </ja>
        /// <en>
        /// Return the text to show on the label.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// カルチャ情報付きのコンストラクタで作成された場合には、<seealso cref="StringResource">StringResource</seealso>
        /// の<see cref="StringResource.GetString">GetStringメソッド</see>が呼び出された結果が戻ります。
        /// </ja>
        /// <en>
        /// The result that the <see cref="StringResource.GetString">GetString method</see> of <seealso cref="StringResource">StringResource</seealso> is called returns when made by the constructor with culture information. 
        /// </en>
        /// </remarks>
        public virtual string Text {
            get {
                return _usingStringResource ? _res.GetString(_text) : _text;
            }
        }

        /// <summary>
        /// <ja>
        /// ラベル幅を返します。単位はピクセルです。
        /// </ja>
        /// <en>
        /// Return the width of the label. The unit is a pixel. 
        /// </en>
        /// </summary>
        public virtual int Width {
            get {
                return _width;
            }
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー要素のボタンを構成する機能を提供します。
    /// </ja>
    /// <en>
    /// Offer the function to compose the button of the toolbar element.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者はこのクラスを用いることで、<seealso cref="IToolBarCommandButton">IToolBarCommandButton</seealso>
    /// を備えたオブジェクトを構成できます。
    /// </ja>
    /// <en>
    /// The developer can compose the object that has <seealso cref="IToolBarCommandButton">IToolBarCommandButton</seealso> by using this class. 
    /// </en>
    /// </remarks>
    public class ToolBarCommandButtonImpl : ToolBarElementImpl, IToolBarCommandButton {
        /// <summary>
        /// <ja>
        /// ツールバーがクリックされたときに実行されるコマンドです。
        /// </ja>
        /// <en>
        /// Command executed when toolbar is clicked.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// コマンドはコンストラクタによって設定され、<seealso cref="Command">Commandプロパティ</seealso>で返されます。
        /// </ja>
        /// <en>
        /// The command is set by the constractor, and return by <seealso cref="Command">Command property</seealso>.
        /// </en>
        /// </remarks>
        protected IPoderosaCommand _command;
        /// <summary>
        /// <ja>
        /// アイコンを保持する内部変数です。アイコンの大きさは16×16ピクセルでなければなりません。
        /// </ja>
        /// <en>
        /// Inside variable that holds the icon. The size of the icon should be 16×16 pixels. 
        /// </en>
        /// </summary>
        /// <ja>
        /// アイコンはコンストラクタによって設定され、<seealso cref="Icon">Iconプロパティ</seealso>で返されます。
        /// </ja>
        /// <en>
        /// The icon is set by the constractor, and return by <seealso cref="Command">Command property</seealso>.
        /// </en>
        protected Image _icon;

        /// <summary>
        /// <ja>
        /// ツールバーのボタンを作成します。
        /// </ja>
        /// <en>
        /// Create the button of the toolbar.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 引数なしのコンストラクタでは、何もコマンドが実行されず、アイコンも設定されません。
        /// </ja>
        /// <en>
        /// For the constructor who doesn't have the argument, as for anything, the command is not executed, and the icon is not set. 
        /// </en>
        /// </remarks>
        public ToolBarCommandButtonImpl() {
        }
        /// <summary>
        /// <ja>
        /// ツールバーのボタンを作成します。
        /// </ja>
        /// <en>
        /// Create the button on the toolbar.
        /// </en>
        /// </summary>
        /// <param name="command"><ja>ボタンがクリックされたときに実行されるコマンドです。</ja><en>Command that execuses when the button is clicked</en></param>
        /// <param name="icon"><ja>ボタンに表示するアイコンです。アイコンの大きさは16×16ドットでなければなりません</ja>
        /// <en>
        /// Icon that show on the button. The size of the icon should be 16×16 pixels. 
        /// </en></param>
        public ToolBarCommandButtonImpl(IPoderosaCommand command, Image icon) {
            _command = command;
            _icon = icon;
        }

        /// <summary>
        /// <ja>
        /// ボタンがクリックされたときに実行するコマンドを示します。
        /// </ja>
        /// <en>
        /// The command executed when the button is clicked is shown. 
        /// </en>
        /// </summary>
        public virtual IPoderosaCommand Command {
            get {
                return _command;
            }
        }

        /// <summary>
        /// <ja>
        /// ボタンに表示するアイコンです。
        /// </ja>
        /// <en>
        /// Icon displayed in button
        /// </en>
        /// </summary>
        public virtual Image Icon {
            get {
                return _icon;
            }
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー要素のコンボボックスを構成する機能を提供します。
    /// </ja>
    /// <en>
    /// Offers the function to compose the combobox of the toolbar element. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// 開発者はこのクラスを用いることで、<seealso cref="IToolBarComboBox">IToolBarComboBox</seealso>
    /// を備えたオブジェクトを構成できます。
    /// </para>
    /// <para>
    /// このクラスは抽象クラスであり、ひな形にすぎません。必要に応じてオーバーライドが必要です。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer can compose the object that has <seealso cref="IToolBarComboBox">IToolBarComboBox</seealso> by using this class. 
    /// </para>
    /// <para>
    /// This class is an abstraction class. Therefore, this is a model. Override is necessary if necessary. 
    /// </para>
    /// </en>
    /// </remarks>
    public abstract class ToolBarComboBoxImpl : ToolBarElementImpl, IToolBarComboBox {
        /// <summary>
        /// <ja>
        /// コンボボックスの選択肢（アイテム）を示す変数です。
        /// </ja>
        /// <en>
        /// Variable that shows item of combobox.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// この値は、<seealso cref="Items">Itemsプロパティ</seealso>から返されます。
        /// </ja>
        /// <en>
        /// This variable is returned from <seealso cref="Items">Items property</seealso>.
        /// </en>
        /// </remarks>
        protected object[] _items;

        /// <summary>
        /// <ja>
        /// コンボボックスの幅です。単位はピクセルです。
        /// </ja>
        /// <en>
        /// Width of the combobox. This unit is pixel.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// この値は、<seealso cref="Width">Widthプロパティ</seealso>から返されます。
        /// </ja>
        /// <en>
        /// This variable is returned from <seealso cref="Items">Width property</seealso>.
        /// </en>
        /// </remarks>
        protected int _width;

        /// <summary>
        /// <ja>
        /// コンボボックスの選択肢（アイテム）を示します。
        /// </ja>
        /// <en>
        /// The item of the combobox is shown. 
        /// </en>
        /// </summary>
        public virtual object[] Items {
            get {
                return _items;
            }
        }

        /// <summary>
        /// <ja>
        /// コンボボックスの幅です。単位はピクセルです。
        /// </ja>
        /// <en>
        /// Return the width of the combobox. The unit is a pixel. 
        /// </en>
        /// </summary>
        public virtual int Width {
            get {
                return _width;
            }
        }

        /// <summary>
        /// <ja>
        /// コンボボックスが選択できるかどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the combobox can be selected. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja>選択できるときにはtrue、そうでないときにはfalseが返されます。</ja><en>False is returned when it is not true so when it is possible to select it. </en></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public virtual bool IsEnabled(ICommandTarget target) {
            return true;
        }

        /// <summary>
        /// <ja>
        /// 現在選択されているアイテムのインデックス番号を返します。
        /// </ja>
        /// <en>
        /// The index number of the item that has been selected now is returned. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja><paramref name="target">target</paramref>のインデックス位置を返します。</ja>
        /// <en>Return index position of the <paramref name="target">target</paramref></en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public abstract int GetSelectedIndex(ICommandTarget target);
        /// <summary>
        /// <ja>
        /// コンボボックスで選択されている選択肢が変化したときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// It is a method of the call when choices that have been selected in the combobox change. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <param name="selectedIndex"><ja>ユーザーが選択したアイテムのインデックス番号です。</ja><en>It is an index number of the item that the user selected. </en></param>
        /// <param name="selectedItem"><ja>ユーザーが選択したアイテムのオブジェクトです。</ja><en>An object of the item that the user selected. </en></param>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public virtual void OnChange(ICommandTarget target, int selectedIndex, object selectedItem) {
        }
    }

    /// <summary>
    /// <ja>
    /// ツールバー要素のトグルボタンを構成する機能を提供します。
    /// </ja>
    /// <en>
    /// Offers the function to compose the toggle button of the toolbar element.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 開発者はこのクラスを用いることで、<seealso cref="IToolBarToggleButton">IToolBarToggleButton</seealso>
    /// を備えたオブジェクトを構成できます。
    /// </ja>
    /// <en>
    /// The developer can compose the object that has <seealso cref="IToolBarToggleButton">IToolBarToggleButton</seealso> by using this class. 
    /// </en>
    /// </remarks>
    public abstract class ToolBarToggleButtonImpl : ToolBarElementImpl, IToolBarToggleButton {
        /// <summary>
        /// <ja>
        /// アイコンを示す内部変数です。アイコンの大きさは16×16ピクセルでなければなりません。
        /// </ja>
        /// <en>
        /// Inside variable that holds the icon. The size of the icon should be 16×16 pixels. 
        /// </en>
        /// </summary>
        protected Image _icon;

        /// <summary>
        /// <ja>
        /// アイコンを返します。
        /// </ja>
        /// <en>
        /// Return the icon.
        /// </en>
        /// </summary>
        public virtual Image Icon {
            get {
                return _icon;
            }
        }

        /// <summary>
        /// <ja>
        /// トグルボタンが選択できるかどうかを示します。
        /// </ja>
        /// <en>
        /// It is shown whether the toggle button can be selected. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja>選択できるときにはtrue、そうでないときにはfalseが返されます。</ja><en>False is returned when it is not true so when it is possible to select it. </en></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public virtual bool IsEnabled(ICommandTarget target) {
            return true;
        }

        /// <summary>
        /// <ja>
        /// トグルボタンのオン／オフの状態を返します。
        /// </ja>
        /// <en>
        /// Return the state of on/off of the toggle button.
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <returns><ja>オンのとき（凹んでいるとき）にはtrue、オフのとき（凹んでいないとき）にはfalseを返します。</ja><en>False is returned when true off (When not denting) when turning it on (When denting). </en></returns>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public virtual bool IsChecked(ICommandTarget target) {
            return false;
        }

        /// <summary>
        /// <ja>
        /// オン／オフの状態が変化したときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// It is a method of the call when the state of on/off changes. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>実行の対象となるターゲットです。</ja><en>Target for execution</en></param>
        /// <param name="is_checked"><ja>オン／オフの状態です。trueのときにはオン（凹んでいる状態）、falseのときにはオフ（凹んでいない状態）です。</ja><en>It is a state of on/off. At on (state that has dented) and false at true, it is off (state that doesn't dent). </en></param>
        /// <remarks>
        /// <ja>
        /// <paramref name="target">target</paramref>は、このツールバーが属する<see cref="IPoderosaMainWindow">IPoderosaMainWindow</see>です。
        /// </ja>
        /// <en>
        /// <paramref name="target">target</paramref> is <see cref="IPoderosaMainWindow">IPoderosaMainWindow</see> that this toolbar belongs. 
        /// </en>
        /// </remarks>
        public abstract void OnChange(ICommandTarget target, bool is_checked);
    }
}
