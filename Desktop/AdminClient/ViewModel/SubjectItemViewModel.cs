﻿using DbModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AdminClient
{
    public class SubjectItemViewModel: CreateSubjectPageViewModel
    {
        #region Public Properties

        /// <summary>
        /// A flag to ignore click command when it is running
        /// </summary>
        public bool CommandIsRunning { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UpdateIsRunning { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Choose ?
        /// </summary>
        public bool IsChosen { get; set; }

        #endregion

        #region Public Commands

        /// <summary>
        /// Click command to show credentials of this subject
        /// </summary>
        public ICommand ClickCommand { get; set; }

        /// <summary>
        /// Update credential of this subject
        /// </summary>
        public ICommand UpdateCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SubjectItemViewModel(): base()
        {
            ClickCommand = new RelayCommand(async () => await Click());
            UpdateCommand = new RelayParameterizedCommand(async (parameter) => await Update(parameter));
        }


        #endregion

        #region Command Methods

        /// <summary>
        /// Click method for ClickCommand
        /// </summary>
        public async Task Click()
        {
            await RunCommand(() => this.CommandIsRunning, async () =>
            {
                if (IsLoaded)
                {
                    CommandIsRunning = false;
                    IsChosen ^= true;
                    return;
                }
                else
                {
                    WebRequestResult<CreateSubjectCredentialsDataModel> response;

                    try
                    {
                        response = await WebRequests.GetAsync<CreateSubjectCredentialsDataModel>($"http://localhost:51197/api/subject/{ID.EditText}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Get subjet failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        CommandIsRunning = false;
                        return;
                    }


                    CreateSubjectCredentialsDataModel result = response.ServerResponse;

                    Credit = new TextEntryViewModel() { Label = "Credit", EditText = result.Subject.Credit.ToString() };
                    DateStart = result.Subject.TimeStart;
                    DateFinish = result.Subject.TimeFinish;

                    foreach (var item in result.Schedule)
                    {
                        var temp = SpecificTimeItems.Items.Where(e => e.DayInTheWeek.Equals(item.DayInTheWeek)).FirstOrDefault();
                        temp.IsChecked = true;
                        temp.Period = item.Period;
                        temp.Room = item.Room;
                        temp.TimeStart = item.TimeStart;
                        temp.TimeFinish = item.TimeFinish;
                    }

                    IsChosen ^= true;
                    IsLoaded = true;
                }
            });
        }

        /// <summary>
        /// Update method for UpdateCommand
        /// </summary>
        /// <returns></returns>
        public async Task Update(object parameter)
        {
            await RunCommand(() => this.UpdateIsRunning, async () =>
               {
                   CreateSubjectCredentialsDataModel DataToPost = ConvertToCredentailsCreate();

                   if (DataToPost == null)
                   {
                       return;
                   }

                   var respone = await WebRequests.PostAsync<ApiResponse<int>>("http://localhost:51197/api/subject/updatesubject", DataToPost);

                   if (respone.DisplayErrorIfFailed("Update subject failed"))
                   {
                       return;
                   }

                   MessageBox.Show("Update subject succeed", "Notify", MessageBoxButton.OK);
               });
        }

        #endregion
    }
}