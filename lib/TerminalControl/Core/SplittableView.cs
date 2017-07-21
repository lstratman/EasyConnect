/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SplittableView.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

using Poderosa.Plugins;
using Poderosa.Forms;
using Poderosa.Sessions;
using Poderosa.Commands;
using Poderosa.UI;
using Poderosa.View;
using Poderosa.Util.Collections;

namespace Poderosa.Forms {
    internal class DefaultViewManagerFactory : IViewManagerFactory {
        private IViewFactory _defaultViewFactory;

        public DefaultViewManagerFactory() {
        }
        //IMainWindowContentFactory
        public IViewManager Create(IPoderosaMainWindow parent) {
            Debug.Assert(parent != null);
            Debug.Assert(_defaultViewFactory != null); //本当はAssertではまずい
            SplittableViewManager pm = new SplittableViewManager(parent, _defaultViewFactory);
            return pm;
        }

        public IViewFactory DefaultViewFactory {
            get {
                return _defaultViewFactory;
            }
            set {
                _defaultViewFactory = value;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    internal class ViewFactoryManager {
        private IViewFactory[] _viewFactories;
        private IExtensionPoint _viewformatChangeHandler;

        public ViewFactoryManager() {
            _viewformatChangeHandler = WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.VIEWFORMATEVENTHANDLER_ID);
        }
        public IExtensionPoint ViewFormatChangeHandler {
            get {
                return _viewformatChangeHandler;
            }
        }

        public IViewFactory GetViewFactoryByView(Type viewclass) {
            LateCheck();
            foreach (IViewFactory vf in _viewFactories) {
                if (vf.GetViewType() == viewclass)
                    return vf;
            }
            throw new ArgumentException("ViewFactory not found: viewclass=" + viewclass.FullName);
        }

        public IViewFactory GetViewFactoryByDoc(Type documentclass) {
            LateCheck();
            foreach (IViewFactory vf in _viewFactories) {
                if (vf.GetDocumentType() == documentclass)
                    return vf;
            }
            throw new ArgumentException("ViewFactory not found: docclass=" + documentclass.FullName);
        }

        //ViewFactoryを遅延作成
        private void LateCheck() {
            IExtensionPoint fs = WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.VIEW_FACTORY_ID);
            _viewFactories = (IViewFactory[])fs.GetExtensions();
            if (_viewFactories.Length == 0)
                throw new Exception("at least one ViewFactory is required");
        }
    }

    internal class SplittableViewManager : ISplittableViewManager, PaneDivision.IUIActionHandler {
        private PaneDivision.IPane _singlePane; //分割していないときにのみ非null
        private PaneDivision _paneDivision;
        private IViewFactory _defaultViewFactory;
        private IPoderosaMainWindow _parent;

        public SplittableViewManager(IPoderosaMainWindow parent, IViewFactory defaultviewfactory) {
            _parent = parent;
            _defaultViewFactory = defaultviewfactory;

            Debug.Assert(_paneDivision == null);
            _singlePane = CreateNewPane(_defaultViewFactory, DockStyle.Fill); //先頭のFactoryで作ってしまうというのはどうかな

            _paneDivision = new PaneDivision();
            _paneDivision.CountLimit = WindowManagerPlugin.Instance.WindowPreference.OriginalPreference.SplitLimitCount;
            _paneDivision.UIActionHandler = this;
        }

        #region IPaneManager
        public IPoderosaView GetCandidateViewForNewDocument() {
            Debug.Assert(_paneDivision != null);
            if (_singlePane != null) {
                Debug.Assert(_paneDivision.IsEmpty);
                return (IPoderosaView)_singlePane;
            }

            IPoderosaView firstPane = null;
            IPoderosaView result = null;
            _paneDivision.FindFirst(delegate(PaneDivision.IPane p) {
                IPoderosaView v = (IPoderosaView)p; //TODO GetAdapterか. 上のキャストもおなじ
                if (firstPane == null)
                    firstPane = v; //先頭

                if (v.Document == null) { //ドキュメントのないビュー優先
                    result = v;
                    return true;
                }
                else
                    return false;
            });

            //空きビューがあればそこへ、なければnull
            if (result != null)
                return result;
            else
                return firstPane;
        }
        public IPoderosaView[] GetAllViews() {
            List<IPoderosaView> result = new List<IPoderosaView>();
            if (_singlePane != null)
                result.Add((IPoderosaView)_singlePane);
            else {
                _paneDivision.FindFirst(delegate(PaneDivision.IPane p) {
                    IPoderosaView view = (IPoderosaView)p;
                    result.Add(view);
                    return false;
                }); //常にfalse返すことで列挙できる
            }
            return result.ToArray();
        }

        public IPoderosaMainWindow ParentWindow {
            get {
                return _parent;
            }
        }
        public CommandResult SplitHorizontal(IContentReplaceableView view, IViewFactory factory) {
            if (factory == null)
                factory = _defaultViewFactory;
            SplitHorizontal((PaneDivision.IPane)view.GetAdapter(typeof(PaneDivision.IPane)), factory);
            return CommandResult.Succeeded;
        }
        public CommandResult SplitVertical(IContentReplaceableView view, IViewFactory factory) {
            if (factory == null)
                factory = _defaultViewFactory;
            SplitVertical((PaneDivision.IPane)view.GetAdapter(typeof(PaneDivision.IPane)), factory);
            return CommandResult.Succeeded;
        }
        public CommandResult Unify(IContentReplaceableView view, out IContentReplaceableView next) {
            PaneDivision.IPane nextfocus = null;
            bool r = Unify((PaneDivision.IPane)view.GetAdapter(typeof(PaneDivision.IPane)), out nextfocus);
            next = r ? (IContentReplaceableView)nextfocus : null; //TODO ちょいまず
            return r ? CommandResult.Succeeded : CommandResult.Failed;
        }
        public CommandResult UnifyAll(out IContentReplaceableView next) {
            PaneDivision.IPane nextfocus = null;
            UnifyAll(out nextfocus);
            next = (IContentReplaceableView)nextfocus;
            return CommandResult.Succeeded;
        }
        public bool CanSplit(IContentReplaceableView view) {
            return _paneDivision.CountLimit >= _paneDivision.PaneCount;
        }
        public bool CanUnify(IContentReplaceableView view) {
            return _paneDivision.PaneCount > 1;
        }
        public bool IsSplitted() {
            return _singlePane == null;
        }
        public string FormatSplitInfo() {
            return _paneDivision.FormatSplit();
        }
        public void ApplySplitInfo(string format) {
            try {
                //現状を獲得
                IPoderosaView[] previous_views = GetAllViews();
                IPoderosaDocument[] documents = new IPoderosaDocument[previous_views.Length]; //分割適用後アクティブになるやつ
                for (int i = 0; i < previous_views.Length; i++)
                    documents[i] = previous_views[i].Document;
                IPoderosaView[] new_views;

                SessionManagerPlugin sm = SessionManagerPlugin.Instance;

                if (format.Length > 0) {
                    Form container = _parent.AsForm();
                    container.SuspendLayout();
                    Control old_root = this.RootControl;
                    _paneDivision.ApplySplitInfo(old_root.Parent, old_root, format,
                        delegate(string label) {
                            return CreateNewPane(_defaultViewFactory, DockStyle.Fill);
                        }); //とりあえずデフォルトファクトリで作成
                    container.ResumeLayout(true);
                    _singlePane = null; //成功裏に終わったときのみ
                    new_views = GetAllViews(); //新しいのを取得
                }
                else {
                    IContentReplaceableView view;
                    UnifyAll(out view);
                    new_views = new IPoderosaView[] { view };
                }

                //既存ドキュメントに再適用
                foreach (DocumentHost dh in sm.GetAllDocumentHosts()) {
                    int index = CollectionUtil.ArrayIndexOf(previous_views, dh.LastAttachedView);
                    if (index != -1) {
                        IPoderosaView new_view = index < new_views.Length ? new_views[index] : new_views[0]; //個数が減ったら先頭に
                        dh.AlternateView(new_view);
                    }
                }

                //もともとActiveだったやつを再適用
                for (int i = 0; i < documents.Length; i++) {
                    if (documents[i] != null)
                        sm.AttachDocumentAndView(documents[i], sm.FindDocumentHost(documents[i]).LastAttachedView); //LastAttachedViewはこの上のループで適用済み
                }

            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }


        public Control RootControl {
            get {
                if (_singlePane == null) { //分割済み
                    Debug.Assert(_paneDivision != null);
                    return _paneDivision.RootControl;
                }
                else { //分割していない
                    Debug.Assert(_paneDivision.IsEmpty);
                    return _singlePane.AsDotNet();
                }
            }
        }
        #endregion

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        //分割・結合メソッド
        public void SplitHorizontal(PaneDivision.IPane view, IViewFactory factory) {
            InternalSplit(view, factory, PaneDivision.Direction.TB);
        }
        public void SplitVertical(PaneDivision.IPane view, IViewFactory factory) {
            InternalSplit(view, factory, PaneDivision.Direction.LR);
        }
        private void InternalSplit(PaneDivision.IPane view, IViewFactory factory, PaneDivision.Direction direction) {
            PaneDivision.IPane t = CreateNewPane(factory, direction == PaneDivision.Direction.LR ? DockStyle.Left : DockStyle.Top);
            Form form = _parent.AsForm();
            form.SuspendLayout();
            _paneDivision.SplitPane(view, t, direction);
            _singlePane = null;
            FireOnSplit();
            form.ResumeLayout(true);
            view.AsDotNet().Focus();
        }

        public bool Unify(PaneDivision.IPane view, out PaneDivision.IPane nextfocus) {
            Form form = _parent.AsForm();
            form.SuspendLayout();
            PaneDivision.SplitResult r = _paneDivision.UnifyPane(view, out nextfocus);
            if (r == PaneDivision.SplitResult.Success)
                view.AsDotNet().Dispose();
            if (_paneDivision.IsEmpty)
                _singlePane = nextfocus;
            form.ResumeLayout(true);
            FireOnUnify();
            return r == PaneDivision.SplitResult.Success;
        }
        public void UnifyAll(out PaneDivision.IPane nextfocus) {
            Form form = _parent.AsForm();
            form.SuspendLayout();
            _singlePane = _paneDivision.UnifyAll();
            form.ResumeLayout(true);
            FireOnUnify();
            nextfocus = _singlePane;
        }

        private Control GetRootControl() {
            if (_singlePane != null)
                return _singlePane.AsDotNet();
            Control c = _paneDivision.RootControl;
            return c;
        }
        private PaneDivision.IPane CreateNewPane(IViewFactory factory, DockStyle dock) {
            PaneDivision.IPane pb = new SplittableViewPane(this, factory.CreateNew(_parent));
            pb.AsDotNet().Dock = dock;
            return pb;
        }

        //PaneDivision.IUIActionHandler
        public void RequestUnify(PaneDivision.IPane unify_target) {
            IPoderosaView view = (IPoderosaView)unify_target;
            ICommandManager cmg = CommandManagerPlugin.Instance;
            cmg.Execute(cmg.Find("org.poderosa.core.window.splitunify"), view);
        }


        //Fire Event
        private void FireOnSplit() {
            foreach (IViewFormatEventHandler eh in WindowManagerPlugin.Instance.ViewFactoryManager.ViewFormatChangeHandler.GetExtensions()) {
                eh.OnSplit(this);
            }
        }
        private void FireOnUnify() {
            foreach (IViewFormatEventHandler eh in WindowManagerPlugin.Instance.ViewFactoryManager.ViewFormatChangeHandler.GetExtensions()) {
                eh.OnUnify(this);
            }
        }

    }

    internal class SplittableViewPane : PaneDivision.IPane, IContentReplaceableView, IGeneralViewCommands {
        private IPoderosaView _content;
        private SplittableViewManager _parent;

        public SplittableViewPane(SplittableViewManager parent, IPoderosaView content) {
            _parent = parent;
            Debug.Assert(content != null);
            _content = content;
            IContentReplaceableViewSite site = (IContentReplaceableViewSite)_content.GetAdapter(typeof(IContentReplaceableViewSite));
            if (site != null)
                site.CurrentContentReplaceableView = this;
        }

        public Control AsDotNet() {
            return _content.AsControl();
        }
        public Control AsControl() {
            return _content.AsControl();
        }

        public string Label {
            get {
                return "terminal";
            }
        }

        public void Dispose() {
        }

        public Size Size {
            get {
                return _content.AsControl().Size;
            }
            set {
                _content.AsControl().Size = value;
            }
        }

        public DockStyle Dock {
            get {
                return _content.AsControl().Dock;
            }
            set {
                _content.AsControl().Dock = value;
            }
        }

        #region IControlReplaceableView
        //Document, CurrentSelectionについては委譲する
        public IPoderosaDocument Document {
            get {
                return _content.Document;
            }
        }
        public ISelection CurrentSelection {
            get {
                return _content.CurrentSelection;
            }
        }

        public IViewManager ViewManager {
            get {
                return _parent;
            }
        }
        public IPoderosaForm ParentForm {
            get {
                return _parent.ParentWindow;
            }
        }

        public IPoderosaView GetCurrentContent() {
            return _content;
        }
        public IPoderosaView AssureViewClass(Type viewclass) {
            if (viewclass == _content.GetType())
                return _content; //ダイナミックな置換は不要、OK！

            IContentReplaceableViewSite site = (IContentReplaceableViewSite)_content.GetAdapter(typeof(IContentReplaceableViewSite));

            Control p = _content.AsControl().Parent;
            p.SuspendLayout();
            if (site != null)
                site.CurrentContentReplaceableView = null; //IContentReplaceableViewSiteが取れるかどうかはオプショナル

            Debug.WriteLineIf(DebugOpt.ViewManagement, String.Format("Replace ViewClass {0} => {1}", _content.GetType().Name, viewclass.Name));
            IPoderosaView newview = CreateView(viewclass);
            UIUtil.ReplaceControl(p, _content.AsControl(), newview.AsControl());
            //旧コントロールにドキュメントがくっついていたら、それを外さないと不整合生じる
            if (_content.Document != null) {
                SessionManagerPlugin.Instance.FindDocumentHost(_content.Document).DetachView();
            }

            _content.AsControl().Dispose();
            _content = newview;
            site = (IContentReplaceableViewSite)newview.GetAdapter(typeof(IContentReplaceableViewSite));
            if (site != null)
                site.CurrentContentReplaceableView = this;

            p.ResumeLayout(true);
            return newview;
        }
        private IPoderosaView CreateView(Type viewclass) {
            IViewFactory vf = WindowManagerPlugin.Instance.ViewFactoryManager.GetViewFactoryByView(viewclass);
            Debug.Assert(vf != null);
            return vf.CreateNew(_parent.ParentWindow);
        }

        //空はTerminalViewで
        public void AssureEmptyViewClass() {
            IViewManagerFactory[] vm = (IViewManagerFactory[])WindowManagerPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(WindowManagerConstants.MAINWINDOWCONTENT_ID).GetExtensions();
            Debug.Assert(vm.Length > 0);
            AssureViewClass(vm[0].DefaultViewFactory.GetViewType());
        }

        #endregion

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            IAdaptable r = WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            if (r != null)
                return r;
            else
                return _content.GetAdapter(adapter); //自身が知らない型は中身側に委譲する。IGeneralViewCommandあたりが該当
        }
        #endregion

        #region IGeneralViewCommand
        public IPoderosaCommand Copy {
            get {
                if (_content == null)
                    return null;
                IGeneralViewCommands v = (IGeneralViewCommands)_content.GetAdapter(typeof(IGeneralViewCommands));
                return v == null ? null : v.Copy;
            }
        }

        public IPoderosaCommand Paste {
            get {
                if (_content == null)
                    return null;
                IGeneralViewCommands v = (IGeneralViewCommands)_content.GetAdapter(typeof(IGeneralViewCommands));
                return v == null ? null : v.Paste;
            }
        }
        #endregion
    }
}
