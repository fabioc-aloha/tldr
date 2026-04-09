// Resolve WPF vs WinForms type ambiguities (UseWindowsForms=true for NotifyIcon)
global using Application = System.Windows.Application;
global using Clipboard = System.Windows.Clipboard;
global using DataFormats = System.Windows.DataFormats;
global using DataObject = System.Windows.DataObject;
global using DragDropEffects = System.Windows.DragDropEffects;
global using DragEventArgs = System.Windows.DragEventArgs;
global using RadioButton = System.Windows.Controls.RadioButton;
