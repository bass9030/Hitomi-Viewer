﻿#pragma checksum "..\..\..\Load_info.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "9BB0E31394DF931C08E2DEE15A731F3EE8F09B03E7874651DAD84CB3CC6F325F"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using Hitomi_Viewer;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Hitomi_Viewer {
    
    
    /// <summary>
    /// Load_info
    /// </summary>
    public partial class Load_info : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button search;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox keyword;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image thumbnail;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label title;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label subtitle;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button View;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Download;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Load_at_page;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Load_info.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox Load_at_page_num;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Hitomi Viewer;component/load_info.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Load_info.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.search = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\Load_info.xaml"
            this.search.Click += new System.Windows.RoutedEventHandler(this.search_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.keyword = ((System.Windows.Controls.TextBox)(target));
            
            #line 18 "..\..\..\Load_info.xaml"
            this.keyword.AddHandler(System.Windows.DataObject.PastingEvent, new System.Windows.DataObjectPastingEventHandler(this.keyword_Pasting));
            
            #line default
            #line hidden
            
            #line 18 "..\..\..\Load_info.xaml"
            this.keyword.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.keyword_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.thumbnail = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.title = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.subtitle = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.View = ((System.Windows.Controls.Button)(target));
            
            #line 58 "..\..\..\Load_info.xaml"
            this.View.Click += new System.Windows.RoutedEventHandler(this.View_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Download = ((System.Windows.Controls.Button)(target));
            
            #line 59 "..\..\..\Load_info.xaml"
            this.Download.Click += new System.Windows.RoutedEventHandler(this.Download_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.Load_at_page = ((System.Windows.Controls.CheckBox)(target));
            
            #line 60 "..\..\..\Load_info.xaml"
            this.Load_at_page.Checked += new System.Windows.RoutedEventHandler(this.Load_at_page_Checked);
            
            #line default
            #line hidden
            
            #line 60 "..\..\..\Load_info.xaml"
            this.Load_at_page.Unchecked += new System.Windows.RoutedEventHandler(this.Load_at_page_Unchecked);
            
            #line default
            #line hidden
            return;
            case 9:
            this.Load_at_page_num = ((System.Windows.Controls.ComboBox)(target));
            
            #line 61 "..\..\..\Load_info.xaml"
            this.Load_at_page_num.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Load_at_page_num_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

