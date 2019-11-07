﻿#region License
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

using System;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.ActionsLib.ActionsConversion;
using Ginger.Actions.ActionConversion;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;

namespace Ginger.Actions.ApiActionsConversion
{
    /// <summary>
    /// This class is used to ApiActionsConversionWizard 
    /// </summary>
    public class ApiActionsConversionWizard : WizardBase, IConversionProcess
    {
        public override string Title { get { return "Convert Webservices Actions"; } }

        private ObservableList<BusinessFlowToConvert> mListOfBusinessFlow = null;
        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlow
        {
            get {
                return mListOfBusinessFlow;
            }
            set
            {
                mListOfBusinessFlow = value;
            }
        }

        public bool ParameterizeRequestBody { get; set; }

        public bool PullValidations { get; set; }

        public eModelConversionType ModelConversionType { get; set; }

        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();
        ConversionStatusReportPage mReportPage = null;
        ApiActionConversionUtils mConversionUtils = new ApiActionConversionUtils();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public ApiActionsConversionWizard(Context context)
        {
            ModelConversionType = eModelConversionType.ApiActionConversion;
            Context = context;
            ListOfBusinessFlow = GetBusinessFlowsToConvert(); 

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Webservices Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ApiActionsConversion/ApiActionsConversionIntro.md"));
            AddPage(Name: "Select Business Flow's for Conversion", Title: "Select Business Flow's for Conversion", SubTitle: "Select Business Flow's for Conversion", Page: new SelectBusinessFlowWzardPage(ListOfBusinessFlow, context));
            AddPage(Name: "API Conversion Configurations", Title: "API Conversion Configurations", SubTitle: "API Conversion Configurations", Page: new ApiConversionConfigurationWzardPage());

            mReportPage = new ConversionStatusReportPage(ListOfBusinessFlow);
            AddPage(Name: "Conversion Status Report", Title: "Conversion Status Report", SubTitle: "Conversion Status Report", Page: mReportPage);
        }

        /// <summary>
        /// This method is used to ge the businessflows to convert
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowsToConvert()
        {
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            ObservableList <BusinessFlowToConvert> lst = new ObservableList<BusinessFlowToConvert>();
            foreach (BusinessFlow bf in businessFlows)
            {
                if (IsWebServiceTargetApplicationInFlow(bf))
                {
                    BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert();
                    flowToConvert.BusinessFlow = bf;
                    flowToConvert.TotalProcessingActionsCount = mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
                    if (flowToConvert.TotalProcessingActionsCount > 0)
                    {
                        lst.Add(flowToConvert);  
                    }
                }
            }
            return lst;
        }

        /// <summary>
        /// This method is used to check if WebService TargetApplication is present in BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        private bool IsWebServiceTargetApplicationInFlow(BusinessFlow bf)
        {
            bool isPresent = false;
            foreach (var ta in bf.TargetApplications)
            {
                isPresent = ta.Name.Contains("Services");
                if(isPresent)
                {
                    break;
                }
            }
            return isPresent;
        }

        /// <summary>
        /// This is finish method which does the finish the wizard functionality
        /// </summary>
        public override void Finish()
        {
        }

        /// <summary>
        /// This method is used to Stop the conversion process in between conversion process
        /// </summary>
        public void StopConversion()
        {
            mConversionUtils.StopConversion();
        }

        /// <summary>
        /// This method is used to convert the action in case of Continue & Re-Convert
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="isReConvert"></param>
        public async void ProcessConversion(ObservableList<BusinessFlowToConvert> lst, bool isReConvert)
        {
            ProcessStarted();
            try
            {
                ObservableList<BusinessFlowToConvert> flowsToConvert = new ObservableList<BusinessFlowToConvert>();
                if (isReConvert)
                {
                    ObservableList<BusinessFlowToConvert> selectedLst = new ObservableList<BusinessFlowToConvert>();
                    foreach (var bf in lst)
                    {
                        if (bf.IsSelected)
                        {
                            bf.BusinessFlow.RestoreFromBackup(true);
                            bf.ConversionStatus = eConversionStatus.Pending;
                            bf.SaveStatus = eConversionSaveStatus.Pending;
                            flowsToConvert.Add(bf);
                        }
                    }
                }
                else
                {
                    flowsToConvert = ListOfBusinessFlow;
                }

                if (flowsToConvert.Count > 0)
                {
                    await Task.Run(() => mConversionUtils.ConvertToApiActionsFromBusinessFlows(flowsToConvert, ParameterizeRequestBody, PullValidations));
                }
                mReportPage.SetButtonsVisibility(true);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
            }
            finally
            {
                ProcessEnded();
            }
        }

        /// <summary>
        /// This method is used to convert the actions
        /// </summary>
        /// <param name="lst"></param>
        public async void BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> lst)
        {
            try
            {
                ProcessStarted();

                await Task.Run(() => mConversionUtils.ConvertToApiActionsFromBusinessFlows(lst, ParameterizeRequestBody, PullValidations));

                mReportPage.SetButtonsVisibility(true);

                ProcessEnded();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
                Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        /// <summary>
        /// This method is used to get the Convertible Actions Count From BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf)
        {
            return mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
        }

        /// <summary>
        /// This method is used to cancle the wizard
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
        }

        public void ConversionProcessEnded()
        {
            ProcessStarted();
        }

        public void ConversionProcessStarted()
        {
            ProcessEnded();
        }
    }
}
