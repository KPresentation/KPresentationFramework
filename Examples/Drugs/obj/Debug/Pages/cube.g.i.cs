﻿#pragma checksum "..\..\..\Pages\cube.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "183B23A85741F8CE321F39D9B05BBBE7"
//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.34014
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

using KPresentationFramework;
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


namespace Drugs.Pages {
    
    
    /// <summary>
    /// cube
    /// </summary>
    public partial class cube : KPresentationFramework.KPage, System.Windows.Markup.IComponentConnector {
        
        
        #line 16 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewport3D viewport3D1;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.PerspectiveCamera camMain;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DirectionalLight dirLightMain;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.ModelVisual3D MyModel;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.MeshGeometry3D meshMain;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DiffuseMaterial matDiffuseMain;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\Pages\cube.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.MatrixTransform3D mat;
        
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
            System.Uri resourceLocater = new System.Uri("/Drugs;component/pages/cube.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\cube.xaml"
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
            
            #line 12 "..\..\..\Pages\cube.xaml"
            ((Drugs.Pages.cube)(target)).ImageGrabStart += new System.EventHandler<System.EventArgs>(this.KPage_ImageGrabStart);
            
            #line default
            #line hidden
            
            #line 13 "..\..\..\Pages\cube.xaml"
            ((Drugs.Pages.cube)(target)).ImageGrabContinue += new System.EventHandler<KRecognizerNS.ImageGrabEventArgs>(this.KPage_ImageGrabContinue);
            
            #line default
            #line hidden
            
            #line 14 "..\..\..\Pages\cube.xaml"
            ((Drugs.Pages.cube)(target)).ImageGrabTerminate += new System.EventHandler<System.EventArgs>(this.KPage_ImageGrabTerminate);
            
            #line default
            #line hidden
            return;
            case 2:
            this.viewport3D1 = ((System.Windows.Controls.Viewport3D)(target));
            return;
            case 3:
            this.camMain = ((System.Windows.Media.Media3D.PerspectiveCamera)(target));
            return;
            case 4:
            this.dirLightMain = ((System.Windows.Media.Media3D.DirectionalLight)(target));
            return;
            case 5:
            this.MyModel = ((System.Windows.Media.Media3D.ModelVisual3D)(target));
            return;
            case 6:
            this.meshMain = ((System.Windows.Media.Media3D.MeshGeometry3D)(target));
            return;
            case 7:
            this.matDiffuseMain = ((System.Windows.Media.Media3D.DiffuseMaterial)(target));
            return;
            case 8:
            this.mat = ((System.Windows.Media.Media3D.MatrixTransform3D)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
