﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages_New;
using Ginger.Drivers.Common;
using Ginger.Drivers.PowerBuilder;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Windows;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyNavAction.xaml
    /// </summary>
    public partial class LiveSpyNavPage : Page
    {
        Context mContext;
        IWindowExplorer mWindowExplorerDriver;
        List<AgentPageMappingHelper> mWinExplorerPageList = null;
        LiveSpyPage CurrentLoadedPage = null;

        public LiveSpyNavPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            context.PropertyChanged += Context_PropertyChanged;
            LoadWindowExplorerPage();
            SetFrameEnableDisable();
        }

        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Context.AgentStatus))
            {
                SetFrameEnableDisable();
                CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
            }
            else if (e.PropertyName == nameof(Context.Agent) && mContext.Agent != null)
            {
                LoadWindowExplorerPage();
                SetFrameEnableDisable();
                CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
            }
        }

        /// <summary>
        /// This method will check if agent is running then it will enable the frame
        /// </summary>
        private void SetFrameEnableDisable()
        {
            bool isAgentRunning = mContext.Agent.Status == Agent.eStatus.Running;                  //  AgentHelper.CheckIfAgentIsRunning(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext, out mWindowExplorerDriver);
            if(mContext.Agent != null)
                mWindowExplorerDriver = mContext.Agent.Driver as IWindowExplorer;

            if (isAgentRunning)
            {
                xSelectedItemFrame.IsEnabled = true;
            }
            else
            {
                xSelectedItemFrame.IsEnabled = false;
            }
        }
        
        /// <summary>
        /// This method is used to get the new WindowExplorerPage based on Context and Agent
        /// </summary>
        /// <returns></returns>
        private void LoadWindowExplorerPage()
        {
            bool isLoaded = false;
            if (mWinExplorerPageList != null && mWinExplorerPageList.Count > 0 && context.Agent != null)
            {
                AgentPageMappingHelper objHelper = mWinExplorerPageList.Find(x => x.ObjectAgent.DriverType == mContext.Agent.DriverType &&
                                                                                x.ObjectAgent.ItemName == mContext.Agent.ItemName);
                if (objHelper != null && objHelper.ObjectWindowPage != null)
                {
                    CurrentLoadedPage = (LiveSpyPage)objHelper.ObjectWindowPage;
                    isLoaded = true;
                }
            }

            if (!isLoaded)
            {
                ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
                if (appAgent != null)
                {
                    CurrentLoadedPage = new LiveSpyPage(mContext);
                    CurrentLoadedPage.SetWindowExplorerForNewPanel(mWindowExplorerDriver);
                    if (mWinExplorerPageList == null)
                    {
                        mWinExplorerPageList = new List<AgentPageMappingHelper>();
                    }
                    mWinExplorerPageList.Add(new AgentPageMappingHelper(mContext.Agent, CurrentLoadedPage));
                }
            }

            xSelectedItemFrame.Content = CurrentLoadedPage;
        }
    }
}
