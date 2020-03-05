#region License
/*
Copyright © 2014-2020 European Support Limited

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
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLFrameTreeItem : HTMLElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "Button16x16.png";  // TODO:replace to black button style
            string Title = this.ElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            list.Add(new ActGenElement()
            {
                Description = "Switch Frame " +  this.ElementInfo.ElementTitle,
                GenElementAction = ActGenElement.eGenElementAction.SwitchFrame
            });
            return list;
        }
    }
}
