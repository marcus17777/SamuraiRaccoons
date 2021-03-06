﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Resources.Core;
using SpeechAndTTS;
using Windows.Media.SpeechSynthesis;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TalkToCorona
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private IAsyncOperation<SpeechRecognitionResult> recognitionOperation;
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        private SpeechSynthesizer synthesizer;
        private Boolean talk = true;
        private String current_sentence;
        private List<String> manners;
        private List<String> zombies;
        private List<List<String>> sentence_list = new List<List<String>>();
        private int sentence_list_index = 0;
        private int sentence_index = 0;


        public MainPage()
        {
            this.InitializeComponent();

            manners = new List<string>();
            manners.Add("Goodbye");
            manners.Add("Hello");
            manners.Add("Thank you");
            manners.Add("How are you");
            manners.Add("Excuse me");
            manners.Add("I am sorry");
            manners.Add("Good afternoon");
            manners.Add("You are welcome");
            manners.Add("Please");
            manners.Add("Bless you");
            manners.Add("My condolences");
            manners.Add("Pardon");
            manners.Add("Good morning");
            manners.Add("How is it going");
            manners.Add("Greetings");

            zombies = new List<string>();
            zombies.Add("Zombie");
            zombies.Add("Fortress");
            zombies.Add("Hammer");
            zombies.Add("Dead");
            zombies.Add("Brutal");
            zombies.Add("Cemetery");
            zombies.Add("Corps");
            zombies.Add("Freakish");
            zombies.Add("Creepy");
            zombies.Add("Curse");
            zombies.Add("Dying");
            zombies.Add("Graveyard");
            zombies.Add("Horrible");
            zombies.Add("Night of the Living Dead");
            zombies.Add("Nightmare");
            zombies.Add("Paleness");
            zombies.Add("Monster");
            zombies.Add("Moon");
            zombies.Add("Obscure");
            zombies.Add("Yelling");
            zombies.Add("Screaming");
            zombies.Add("Superstition");
            zombies.Add("Wicked");
            zombies.Add("Walking corpse");
            zombies.Add("Undead");
            zombies.Add("Transformation");
            zombies.Add("Petrify");
            zombies.Add("Disturbing");
            zombies.Add("Lurkers");
            zombies.Add("Creepy Crawlies");
            zombies.Add("Crimson Heads");
            zombies.Add("Necrotics");
            zombies.Add("The Walking Dead");

            sentence_list.Add(manners);
            sentence_list.Add(zombies);

            synthesizer = new SpeechSynthesizer();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Save the UI thread dispatcher to allow speech status messages to be shown on the UI.
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                // Enable the recognition buttons.
                btnRecognizeWithoutUI.IsEnabled = true;

                Language speechLanguage = SpeechRecognizer.SystemSpeechLanguage;
                speechContext = ResourceContext.GetForCurrentView();
                speechContext.Languages = new string[] { speechLanguage.LanguageTag };

                speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationSpeechResources");

                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);

                await say_things("Hello, I'm Corona, your personal english teacher. I can help you learn pronunciating different sentences.");

            }
            else
            {
                resultTextBlock.Visibility = Visibility.Visible;
                resultTextBlock.Text = "Permission to access capture resources was not given by the user; please set the application setting in Settings->Privacy->Microphone.";
                btnRecognizeWithoutUI.IsEnabled = false;
            }
        }

        private async Task say_things(String sentence)
        {
            try
            {
                SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(sentence);
                // Set the source and start playing the synthesized audio stream.
                media.AutoPlay = true;
                media.SetSource(synthesisStream, synthesisStream.ContentType);
                media.Play();
                // Create a stream from the text. This will be played using a media element.

            }
            catch (System.IO.FileNotFoundException)
            {
                // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                // be able to start media playback. Handle this gracefully
                var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                await messageDialog.ShowAsync();
            }
            catch (Exception)
            {
                // If the text is unable to be synthesized, throw an error message to the user.
                media.AutoPlay = false;
                var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                await messageDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            // Create an instance of SpeechRecognizer.
            speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            // Provide feedback to the user about the state of the recognizer.
            speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

            // Add a web search topic constraint to the recognizer.
            var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "webSearch");
            speechRecognizer.Constraints.Add(webSearchGrammar);

            // RecognizeWithUIAsync allows developers to customize the prompts.    
            speechRecognizer.UIOptions.AudiblePrompt = "Say what you want to search for...";
            speechRecognizer.UIOptions.ExampleText = speechResourceMap.GetValue("WebSearchUIOptionsExampleText", speechContext).ValueAsString;

            // Compile the constraint.
            SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

            // Check to make sure that the constraints were in a proper format and the recognizer was able to compile it.
            if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
            {
                // Disable the recognition buttons.

                btnRecognizeWithoutUI.IsEnabled = false;

                // Let the user know that the grammar didn't compile properly.
                resultTextBlock.Visibility = Visibility.Visible;
                resultTextBlock.Text = "Unable to compile grammar.";
            }
        }

        /// <summary>
        /// Ensure that we clean up any state tracking event handlers created in OnNavigatedTo to prevent leaks.
        /// </summary>
        /// <param name="e">Details about the navigation event</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    if (recognitionOperation != null)
                    {
                        recognitionOperation.Cancel();
                        recognitionOperation = null;
                    }
                }

                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

        }

        /// <summary>
        /// Handle SpeechRecognizer state changed events by updating a UI component.
        /// </summary>
        /// <param name="sender">Speech recognizer that generated this status event</param>
        /// <param name="args">The recognizer's status</param>
        private async void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //Page.Current.NotifyUser("Speech recognizer state: " + args.State.ToString(), NotifyType.StatusMessage);
            });
        }

        /// <summary>
        /// Uses the recognizer constructed earlier to listen for speech from the user before displaying 
        /// it back on the screen. Uses developer-provided UI for user feedback.
        /// </summary>
        /// <param name="sender">Button that triggered this event</param>
        /// <param name="e">State information about the routed event</param>
        private async Task RecognizeWithoutUIWebSearchGrammar_Click(String sentence)
        {

            // Disable the UI while recognition is occurring, and provide feedback to the user about current state.
            btnRecognizeWithoutUI.IsEnabled = false;
            hlOpenPrivacySettings.Visibility = Visibility.Collapsed;
            listenWithoutUIButtonText.Text = " listening for speech...";



            // Start recognition.
            try
            {
                recognitionOperation = speechRecognizer.RecognizeAsync();
                SpeechRecognitionResult speechRecognitionResult = await recognitionOperation;
                // If successful, display the recognition result.
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    //resultTextBlock.Text = speechRecognitionResult.Text;
                    if (speechRecognitionResult.Text.ToLower().Equals(sentence.ToLower()))
                    {
                        resultTextBlock.Visibility = Visibility.Visible;
                        resultTextBlock.Text = "Success" + ", you said: " + speechRecognitionResult.Text;
                        say_things("Success! Great Work");
                    } else if (speechRecognitionResult.Text.ToLower().Equals("change topic"))
                    {
                        // loops back around to first sentence list if reaches the end
                        sentence_list_index = (sentence_list_index + 1) % sentence_list.Count;
                        sentence_index = 0;
                        say_things("Topic changed!");
                    }
                    else
                    {
                        resultTextBlock.Text = "Sorry you FAILED. You said : " + speechRecognitionResult.Text;
                        say_things("FAIL! You said " + speechRecognitionResult.Text + ". Press the button again and try saying the phrase again");
                        btnRecognizeWithoutUI.IsEnabled = true;
                    }
                }
                else
                {
                    resultTextBlock.Visibility = Visibility.Visible;
                    resultTextBlock.Text = string.Format("Speech Recognition Failed, Status: {0}", speechRecognitionResult.Status.ToString());
                }
            }
            catch (TaskCanceledException exception)
            {
                // TaskCanceledException will be thrown if you exit the scenario while the recognizer is actively
                // processing speech. Since this happens here when we navigate out of the scenario, don't try to 
                // show a message dialog for this exception.
                System.Diagnostics.Debug.WriteLine("TaskCanceledException caught while recognition in progress (can be ignored):");
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
            catch (Exception exception)
            {
                // Handle the speech privacy policy error.
                if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                {
                    hlOpenPrivacySettings.Visibility = Visibility.Visible;
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }

            // Reset UI state.
            btnRecognizeWithoutUI.IsEnabled = true;
        }

        /// <summary>
        /// Open the Speech, Inking and Typing page under Settings -> Privacy, enabling a user to accept the 
        /// Microsoft Privacy Policy, and enable personalization.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="args">Ignored</param>
        /// 

        private async void Button_command(object sender, RoutedEventArgs e)
        {
            
            if (talk)
            {
                
                
                current_sentence = sentence_list[sentence_list_index][sentence_index];
                say_things(current_sentence);

                talk = false;
                listenWithoutUIButtonText.Text = " Record!";

                // iterate over indices; loops back around if reaches the end of word list
                sentence_index = sentence_index + 1 % sentence_list[sentence_list_index].Count;
            } else if (!talk)
            {
                await RecognizeWithoutUIWebSearchGrammar_Click(current_sentence);
                listenWithoutUIButtonText.Text = " Say another word.";
                talk = true;
            }
        }
        private async void openPrivacySettings_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping"));
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }
    }
}
