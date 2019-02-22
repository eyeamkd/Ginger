#region License
/*
Copyright © 2014-2018 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for SelectActionWzardPage.xaml
    /// </summary>
    public partial class SelectActionWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        
        public SelectActionWzardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    Init();
                    break;
                case EventType.Finish:
                    break;
            }
        }
        
        private void Init()
        {
            // clearing the list of actions to be converted before clicking on Convertible Actions buttons again to reflect the fresh list of convertible actions
            mWizard.ActionToBeConverted.Clear();

            // fetching list of selected convertible activities from the first grid
            List<Activity> lstSelectedActivities = mWizard.BusinessFlow.Activities.Where(x => x.SelectedForConversion).ToList();

            if (lstSelectedActivities.Count != 0)
            {
                foreach (Activity convertibleActivity in lstSelectedActivities)
                {
                    int count = 1;
                    foreach (Act act in convertibleActivity.Acts)
                    {
                        if ((act is IObsoleteAction) && (((IObsoleteAction)act).IsObsoleteForPlatform(act.Platform)) &&
                            (act.Active))
                        {
                            ActionConversionHandler newConvertibleActionType = new ActionConversionHandler();
                            newConvertibleActionType.SourceActionTypeName = act.ActionDescription.ToString();
                            newConvertibleActionType.SourceActionType = act.GetType();
                            newConvertibleActionType.TargetActionType = ((IObsoleteAction)act).TargetAction();
                            if (newConvertibleActionType.TargetActionType == null)
                                continue;
                            newConvertibleActionType.TargetActionTypeName = ((IObsoleteAction)act).TargetActionTypeName();
                            newConvertibleActionType.ActionCount = count;
                            newConvertibleActionType.Actions.Add(act);
                            newConvertibleActionType.ActivityList.Add(convertibleActivity.ActivityName);
                            mWizard.ActionToBeConverted.Add(newConvertibleActionType);
                            count++;
                        }
                    }
                }
                if (mWizard.ActionToBeConverted.Count != 0)
                {
                    xGridConvertibleActions.DataSourceList = mWizard.ActionToBeConverted;
                    SetGridView();
                    return;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoConvertibleActionsFound);
                    return;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoActivitySelectedForConversion);
            }
        }

        private void SetGridView()
        {
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActionConversionHandler.Fields.Selected, Header = "Select", WidthWeight = 3.5, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = ActionConversionHandler.Fields.SourceActionTypeName, WidthWeight = 15, Header = "Source Action Type" });
            view.GridColsView.Add(new GridColView() { Field = ActionConversionHandler.Fields.Activities, WidthWeight = 15, Header = "Source " + GingerDicser.GetTermResValue(eTermResKey.Activities) });
            view.GridColsView.Add(new GridColView() { Field = ActionConversionHandler.Fields.TargetActionTypeName, WidthWeight = 15, Header = "Target Action Type" });
            xGridConvertibleActions.SetAllColumnsDefaultView(view);
            xGridConvertibleActions.InitViewItems();
            xGridConvertibleActions.SetTitleLightStyle = true;
            xGridConvertibleActions.ValidationRules = new List<ucGrid.eUcGridValidationRules>()
            {
                ucGrid.eUcGridValidationRules.CheckedRowCount
            };
        }
    }
}
