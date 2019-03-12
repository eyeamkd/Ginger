#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using System.Windows.Controls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Java
{
    class JavaCheckBoxTreeItem : JavaElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Checkbox_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            list.Add(new ActJavaElement()
            {
                Description = "Set " + Name + " ON",
                ControlAction = ActJavaElement.eControlAction.SetValue,
                Value="true"
            });

            list.Add(new ActJavaElement()
            {
                Description = "Set " + Name + " OFF",
                ControlAction = ActJavaElement.eControlAction.SetValue,
                Value="false"
            }); 
            
            list.Add(new ActJavaElement()
            {
                Description = "Toggle Checkbox " + Name,
                ControlAction = ActJavaElement.eControlAction.Toggle
            });

            list.Add(new ActJavaElement()
            {
                Description = "Get " + Name + " Value",
                ControlAction = ActJavaElement.eControlAction.GetValue
            });

            list.Add(new ActJavaElement()
            {
                Description = "Get IsEnabled Property " + Name,
                ControlAction = ActJavaElement.eControlAction.IsEnabled
            });

            list.Add(new ActJavaElement()
            {
                Description = "Is Checked " + Name,
                ControlAction = ActJavaElement.eControlAction.IsChecked
            });
            return list;
        }
    }
}
