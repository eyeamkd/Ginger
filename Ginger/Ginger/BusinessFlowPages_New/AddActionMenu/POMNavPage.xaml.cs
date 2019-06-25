﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.Help;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
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
using static Ginger.ApplicationModelsLib.POMModels.PomElementsPage;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for POMNavAction.xaml
    /// </summary>
    public partial class POMNavPage : Page
    {
        public PomElementsPage mappedUIElementsPage;
        ApplicationPOMModel mPOM;
        Context mContext;
        ITreeViewItem mItemTypeRootNode;
        SingleItemTreeViewSelectionPage mPOMPage;

        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (xMainElementsGrid.Grid.SelectedItem != null)
                {
                    return (ElementInfo)xMainElementsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public POMNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;

            mItemTypeRootNode = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            mPOMPage = new SingleItemTreeViewSelectionPage("Page Object Models", eImageType.ApplicationPOMModel, mItemTypeRootNode, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                        new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication),
                                            UCTreeView.eFilteroperationType.Equals);

            mContext.PropertyChanged += MContext_PropertyChanged;
            mPOMPage.OnSelect += MainTreeView_ItemSelected;
            SetElementsGridView();
            xPOMFrame.Content = mPOMPage;
        }

        //public POMNavPage(Context context, string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addHandler = null, EventHandler treeItemDoubleClickHandler = null)
        //{
        //    InitializeComponent();

        //    mContext = context;
        //    mItemTypeRootNode = itemTypeRootNode;
        //    GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

        //    xTreeView.TreeTitle = itemTypeName;
        //    xTreeView.TreeIcon = itemTypeIcon;

        //    mContext.PropertyChanged -= MContext_PropertyChanged;
        //    mContext.PropertyChanged += MContext_PropertyChanged;

        //    xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
        //    xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
        //    TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);

        //    r.IsExpanded = true;

        //    itemTypeRootNode.SetTools(xTreeView);
        //    xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);

        //    xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;
        //    SetElementsGridView();

            //if (treeItemDoubleClickHandler != null)
            //{
            //    xTreeView.Tree.ItemDoubleClick += treeItemDoubleClickHandler;
            //}
//        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(mContext.BusinessFlow) || e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
            {
                UpdatePOMTree();
            }
            if (e.PropertyName is nameof(mContext.Agent) || e.PropertyName is nameof(mContext.AgentStatus))
            {
                mAgent = mContext.Agent;
            }
        }

        private void UpdatePOMTree()
        {
            if (mContext.BusinessFlow.CurrentActivity == null)
                mContext.BusinessFlow.CurrentActivity = mContext.BusinessFlow.Activities[0];

            mPOMPage.xTreeView.Tree.TreeNodesFilterByField = new Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." + nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), mContext.BusinessFlow.CurrentActivity.TargetApplication);
            mPOMPage.xTreeView.Tree.FilterType = UCTreeView.eFilteroperationType.Equals;
            mPOMPage.xTreeView.Tree.RefresTreeNodeChildrens(mItemTypeRootNode);
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        //public POMNavPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, RoutedEventHandler saveAllHandler = null, RoutedEventHandler addItemHandler = null, EventHandler itemDoubleClickHandler = null)
        //{
        //    InitializeComponent();

        //    GingerHelpProvider.SetHelpString(this, itemTypeName.TrimEnd(new char[] { 's' }));

        //    xTreeView.TreeTitle = itemTypeName;
        //    xTreeView.TreeIcon = itemTypeIcon;

        //    TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);
        //    r.IsExpanded = true;

        //    itemTypeRootNode.SetTools(xTreeView);
        //    xTreeView.SetTopToolBarTools(saveAllHandler, addHandler);

        //    xTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;

        //    SetElementsGridView();
        //    //if (treeItemDoubleClickHandler != null)
        //    //{
        //    //    xTreeView.Tree.ItemDoubleClick += treeItemDoubleClickHandler;
        //    //}
        //}

        private void MainTreeView_ItemSelected(object sender, SelectionTreeEventArgs e)
        {
            GridLength POMDetailsRegionHeight = new GridLength(400, GridUnitType.Star);
            GridLength unloadedPOMDetailsHeight = new GridLength(0);

            TreeViewItem TVI = (TreeViewItem)e.SelectedItems[0];
            object tvItem = TVI.Tag;
            ITreeViewItem mPOMObj = tvItem as ITreeViewItem;

            ApplicationPOMModel mPOM = mPOMObj.NodeObject() as ApplicationPOMModel;
            if (tvItem is ITreeViewItem)
            {
                if (mPOM is ApplicationPOMModel)
                {
                    if (xPOMDetails.Height.Value < POMDetailsRegionHeight.Value)
                        xPOMDetails.Height = POMDetailsRegionHeight;

                    xPOMItems.Height = new GridLength(400, GridUnitType.Auto);
                    xMainElementsGrid.Visibility = Visibility.Visible;
                    foreach(ElementInfo elem in mPOM.MappedUIElements)
                    {
                        elem.ParentGuid = mPOM.Guid;
                    }
                    xMainElementsGrid.DataSourceList = mPOM.MappedUIElements;
                }
                else
                {
                    xMainElementsGrid.Visibility = Visibility.Collapsed;
                    xPOMDetails.Height = unloadedPOMDetailsHeight;
                    xPOMItems.Height = POMDetailsRegionHeight;
                }
                //ApplicationPOMModel appPOM = tvItem as ApplicationPOMModel
                //mPomAllElementsPage = new PomAllElementsPage(appPOM, this);
                //xPOMLDetailsFrame.Content = ((ITreeViewItem)tvItem).EditPage();
            }
            else
            {
                //DetailsFrame.Content = "View/Edit page is not available yet for the tree item '" + tvItem.GetType().Name + "'";
            }
        }

        private void SetElementsGridView()
        {
            xMainElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 25, AllowSorting = true });

            List<GingerCore.GeneralLib.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true });
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            xMainElementsGrid.AddToolbarTool(eImageType.GoBack, "Add to Actions", new RoutedEventHandler(AddFromPOMNavPage));
        }

        private void AddFromPOMNavPage(object sender, RoutedEventArgs e)
        {
            if (xMainElementsGrid.Grid.SelectedItems != null && xMainElementsGrid.Grid.SelectedItems.Count > 0)
            {
                foreach (ElementInfo elemInfo in xMainElementsGrid.Grid.SelectedItems)
                {
                    Act instance = GenerateRelatedAction(elemInfo);
                    if(instance != null)
                    {
                        instance.Active = true;

                    }
                    mContext.BusinessFlow.AddAct(instance, true);
                }

                int selectedActIndex = -1;
                ObservableList<IAct> actsList = mContext.BusinessFlow.CurrentActivity.Acts;
                if (actsList.CurrentItem != null)
                {
                    selectedActIndex = actsList.IndexOf((Act)actsList.CurrentItem);
                }
                if (selectedActIndex >= 0)
                {
                    actsList.Move(actsList.Count - 1, selectedActIndex + 1);

                }
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private Act GenerateRelatedAction(ElementInfo elementInfo)
        {
            Act instance;
            IPlatformInfo mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);              // PlatformInfoBase.GetPlatformImpl(ePlatformType.Web);
            ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
            {
                LocateBy = eLocateBy.POMElement,
                LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                ElementValue = "",
                AddPOMToAction = true,
                POMGuid = elementInfo.ParentGuid.ToString(),
                ElementGuid = elementInfo.Guid.ToString(),
                LearnedElementInfo = elementInfo,
            };

            instance = mPlatform.GetPlatformAction(elementInfo, actionConfigurations);
            return instance;
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.HighLightElement(mSelectedElement, true);
            }
        }
        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return false;
            }

            return true;
        }
        private bool IsDriverBusy()
        {
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
