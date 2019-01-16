﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ALM_Common.DataContracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.JIRA;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Platforms;
using System.IO;


namespace Ginger.ALM.Repository
{
    class JIRA_Repository : ALMRepository
    {

        public ALMCore AlmCore { get; set; }

        public JIRA_Repository(ALMCore almCore)
        {
            this.AlmCore = almCore;
        }
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Connecting to Jira server");
            try
            {
                isConnectSucc = this.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error connecting to Jira server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Could not connect to Jira server");
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailure);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings);
            }

            return isConnectSucc;
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            bool result = false;
            string responseStr=string.Empty;
            if (activtiesGroup != null)
            {
            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(App.UserProfile.Solution.ExternalItemsFields);
                var testCaseFields = allFields.Where(a => a.ItemType == ResourceType.TEST_CASE.ToString());
                bool exportRes = ((JiraCore)this.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, testCaseFields, ref responseStr);

                Reporter.CloseGingerHelper();
                if (exportRes)
                {
                    if (performSaveAfterExport)
                    {
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);
                        Reporter.CloseGingerHelper();
                    }
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
                    return true;
                }
                else
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, responseStr);
            }
            return result;
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            bool askToSaveBF = false;
            foreach (ActivitiesGroup group in grdActivitiesGroups)
            {
                if (ExportActivitiesGroupToALM(group))
                    askToSaveBF = true;
            }

            if (askToSaveBF)
                if (Reporter.ToUser(eUserMsgKeys.AskIfToSaveBFAfterExport, businessFlow.Name) == MessageBoxResult.Yes)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name,
                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.CloseGingerHelper();
                }
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            bool result = false;
            string responseStr = string.Empty;

            if (businessFlow != null)
            {
                if (businessFlow.ActivitiesGroups.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                    return false;
                }
                else
                {
                    ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(App.UserProfile.Solution.ExternalItemsFields);
                    ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);
                    var testCaseFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_CASE.ToString())&&(a.ToUpdate || a.Mandatory));
                    var testSetFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_SET.ToString()) && (a.ToUpdate || a.Mandatory));
                    var testExecutionFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_CASE_EXECUTION_RECORDS.ToString()) && (a.ToUpdate || a.Mandatory));
                    var exportRes = ((JiraCore)this.AlmCore).ExportBfToAlm(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr);
                    if (exportRes)
                    {
                        if (performSaveAfterExport)
                        {
                            Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                            Reporter.CloseGingerHelper();
                        }
                        if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                            Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
                        return true;
                    }
                    else
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                        Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, responseStr);
                }
            }
            return result;
        }

        public override eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKeys.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            return ((JiraCore)this.AlmCore).GetJiraTestSets();
        }

        public override object GetTSRunStatus(object tsItem)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            JIRA.JiraImportReviewPage win = new JIRA.JiraImportReviewPage();
            win.ShowAsWindow();
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            JIRA.JiraImportSetByIdPage win = new JIRA.JiraImportSetByIdPage();
            win.ShowAsWindow();
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<object> selectedTests)
        {
            if (selectedTests != null && selectedTests.Count() > 0)
            {
                ObservableList<JiraTestSet> testSetsItemsToImport = new ObservableList<JiraTestSet>();
                foreach (GingerCore.ALM.JIRA.JiraTestSet selectedTS in selectedTests)
                {
                    try
                    {
                        BusinessFlow existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == selectedTS.Key).FirstOrDefault();
                        if (existedBF != null)
                        {
                            MessageBoxResult userSelection = Reporter.ToUser(eUserMsgKeys.TestSetExists, selectedTS.Name);
                            if (userSelection == MessageBoxResult.Yes)
                            {
                                File.Delete(existedBF.FileName);
                            }
                        }
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.ALMTestSetImport, null, selectedTS.Name);
                        JiraTestSet jiraImportedTSData = ((JiraCore)this.AlmCore).GetJiraTestSetData(selectedTS);
                        
                        SetImportedTS(jiraImportedTSData);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKeys.ErrorInTestsetImport, selectedTS.Name, ex.Message);
                    }
                }
                Reporter.ToUser(eUserMsgKeys.TestSetsImportedSuccessfully);
                return true;
            }
            return false;
        }

        private void SetImportedTS(JiraTestSet importedTS)
        {
            ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            try
            {
                //import test set data
                Reporter.ToGingerHelper(eGingerHelperMsgKey.ALMTestSetImport, null, importedTS.Name);
                BusinessFlow tsBusFlow = ((JiraCore)ALMIntegration.Instance.AlmCore).ConvertJiraTestSetToBF(importedTS);
                if (App.UserProfile.Solution.MainApplication != null)
                {
                    //add the applications mapped to the Activities
                    foreach (Activity activ in tsBusFlow.Activities)
                        if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                            if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                            {
                                ApplicationPlatform appAgent = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                if (appAgent != null)
                                    tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                            }
                    //handle non mapped Activities
                    if (tsBusFlow.TargetApplications.Count == 0)
                        tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
                    foreach (Activity activ in tsBusFlow.Activities)
                        if (string.IsNullOrEmpty(activ.TargetApplication))
                            activ.TargetApplication = tsBusFlow.MainApplication;
                }
                else
                {
                    foreach (Activity activ in tsBusFlow.Activities)
                        activ.TargetApplication = null; // no app configured on solution level
                }

                //save bf
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tsBusFlow);
                Reporter.CloseGingerHelper();
            }
            catch { }
            
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestLabPath()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            return true;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            throw new NotImplementedException();
        }
    }
}
