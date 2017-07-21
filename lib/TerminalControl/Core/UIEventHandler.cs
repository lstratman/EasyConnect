/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: UIEventHandler.cs,v 1.3 2011/12/17 09:49:44 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

//マウスのイベント優先度の管理機能
//　.NETのOnMouseMoveをコントロールの継承関係で受けても構造的にキビシイので
namespace Poderosa.View {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum UIHandleResult {
        Pass,         //次の優先度のハンドラに渡す
        Stop,         //処理を終了する
        Capture,      //自分が優先権を獲得する
        EndCapture    //優先権を放棄する
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IUIHandler {
        string Name {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IMouseHandler : IUIHandler {
        UIHandleResult OnMouseDown(MouseEventArgs args);
        UIHandleResult OnMouseMove(MouseEventArgs args);
        UIHandleResult OnMouseUp(MouseEventArgs args);
        UIHandleResult OnMouseWheel(MouseEventArgs args);
    }

    //ProcessCmdKey/ProcessDialogKeyの周辺に関しての処理を行う
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IKeyHandler : IUIHandler {
        UIHandleResult OnKeyProcess(Keys key);
    }

    //空実装
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public abstract class DefaultMouseHandler : IMouseHandler {
        private string _name;
        public DefaultMouseHandler(string name) {
            _name = name;
        }
        public string Name {
            get {
                return _name;
            }
        }

        public virtual UIHandleResult OnMouseDown(MouseEventArgs args) {
            return UIHandleResult.Pass;
        }

        public virtual UIHandleResult OnMouseMove(MouseEventArgs args) {
            return UIHandleResult.Pass;
        }

        public virtual UIHandleResult OnMouseUp(MouseEventArgs args) {
            return UIHandleResult.Pass;
        }

        public virtual UIHandleResult OnMouseWheel(MouseEventArgs args) {
            return UIHandleResult.Pass;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="HANDLER"></typeparam>
    /// <typeparam name="ARG"></typeparam>
    /// <exclude/>
    public abstract class UIHandlerManager<HANDLER, ARG> where HANDLER : class, IUIHandler {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exclude/>
        public delegate UIHandleResult HandlerDelegate(HANDLER handler, ARG args);

        private List<HANDLER> _handlers; //先頭が最高優先度
        private HANDLER _capturingHandler; //イベントをキャプチャしているハンドラ。存在しないときはnull

        public UIHandlerManager() {
            _handlers = new List<HANDLER>();
        }
        public void AddLastHandler(HANDLER handler) {
            _handlers.Add(handler);
        }
        public void AddFirstHandler(HANDLER handler) {
            _handlers.Insert(0, handler);
        }
        //外部の要請でキャプチャを解除したいこともある。
        public void EndCapture() {
            _capturingHandler = null;
        }
        public HANDLER CapturingHandler {
            get {
                return _capturingHandler;
            }
        }

        //WinFormsのイベントハンドラとの関連付け OnXXXをいちいちoverrideしたくないのでイベントハンドラで行う
        public abstract void AttachControl(Control c);

        //ダンプ
        public string DumpHandlerList() {
            StringBuilder bld = new StringBuilder();
            foreach (IUIHandler h in _handlers) {
                if (bld.Length > 0)
                    bld.Append(',');
                bld.Append(h.Name);
            }
            return bld.ToString();
        }

        //動作の本体
        protected UIHandleResult Process(HandlerDelegate action, ARG args) {
            try {
                if (_capturingHandler != null) {
                    UIHandleResult r = action(_capturingHandler, args);
                    if (r == UIHandleResult.EndCapture)
                        _capturingHandler = null; //キャプチャの終了
                    return r;
                }
                else {
                    //序列の順にまわしていく
                    foreach (HANDLER h in _handlers) {
                        UIHandleResult r = action(h, args);
                        Debug.Assert(r != UIHandleResult.EndCapture);
                        if (r == UIHandleResult.Stop)
                            return r;
                        if (r == UIHandleResult.Capture) {
                            Debug.Assert(_capturingHandler == null);
                            _capturingHandler = h;
                            return r;
                        }
                    }

                }
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }

            return UIHandleResult.Pass;
        }
    }

    //ハンドラのマネージャ
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class MouseHandlerManager : UIHandlerManager<IMouseHandler, MouseEventArgs> {

        //内部使用のデリゲート
        private static HandlerDelegate _mouseDownDelegate =
            delegate(IMouseHandler handler, MouseEventArgs args) {
                return handler.OnMouseDown(args);
            };
        private static HandlerDelegate _mouseUpDelegate =
            delegate(IMouseHandler handler, MouseEventArgs args) {
                return handler.OnMouseUp(args);
            };
        private static HandlerDelegate _mouseMoveDelegate =
            delegate(IMouseHandler handler, MouseEventArgs args) {
                return handler.OnMouseMove(args);
            };
        private static HandlerDelegate _mouseWheelDelegate =
            delegate(IMouseHandler handler, MouseEventArgs args) {
                return handler.OnMouseWheel(args);
            };

        public override void AttachControl(Control c) {
            c.MouseDown += new MouseEventHandler(RootMouseDown);
            c.MouseUp += new MouseEventHandler(RootMouseUp);
            c.MouseMove += new MouseEventHandler(RootMouseMove);
            c.MouseWheel += new MouseEventHandler(RootMouseWheel);
        }

        //WinFormsのイベントハンドラ
        private void RootMouseDown(object sender, MouseEventArgs args) {
            Process(_mouseDownDelegate, args);
        }
        private void RootMouseUp(object sender, MouseEventArgs args) {
            Process(_mouseUpDelegate, args);
        }
        private void RootMouseMove(object sender, MouseEventArgs args) {
            Process(_mouseMoveDelegate, args);
        }
        private void RootMouseWheel(object sender, MouseEventArgs args) {
            Process(_mouseWheelDelegate, args);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class KeyboardHandlerManager : UIHandlerManager<IKeyHandler, Keys> {

        private static HandlerDelegate _keyDelegate =
            delegate(IKeyHandler handler, Keys key) {
                return handler.OnKeyProcess(key);
            };

        public override void AttachControl(Control c) {
            //ProcessDialogKeyがイベントで取れるといいんだが、それはできないので空実装
        }

        //これを外から呼び出す
        public UIHandleResult Process(Keys key) {
            return base.Process(_keyDelegate, key);
        }
    }

}
