using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace GateSale
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            
            // Using a fallback logo placeholder (ellipse with G) defined in XAML
            // No need to load an image file anymore
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await StartSplashAnimation();
        }

        private async Task StartSplashAnimation()
        {
            try
            {
                // Start all animations concurrently
                var animationTasks = new[]
                {
                    AnimateMainContainer(),
                    AnimateLogoContainer(),
                    AnimateAppNameText(),
                    AnimateDecorativeElements(),
                    AnimateParticles(),
                    AnimateProgressBar()
                };

                // Wait for all animations to complete
                await Task.WhenAll(animationTasks);

                // Wait for a moment to show the complete splash screen
                await Task.Delay(2000);

                // Navigate to main page with transition
                await NavigateToMainPage();
            }
            catch (Exception ex)
            {
                // Fallback: navigate to main page immediately if animations fail
                System.Diagnostics.Debug.WriteLine($"Splash animation error: {ex.Message}");
                await NavigateToMainPage();
            }
        }

        private async Task AnimateMainContainer()
        {
            // Fade in and scale up the main container with smooth easing
            await Task.WhenAll(
                MainContainer.FadeTo(1, 1000, Easing.CubicOut),
                MainContainer.ScaleTo(1, 1000, Easing.CubicOut)
            );
        }

        private async Task AnimateLogoContainer()
        {
            // Initial delay before logo animation
            await Task.Delay(300);
            
            // Fade in and scale up logo container with elegant shadow
            await Task.WhenAll(
                LogoContainer.FadeTo(1, 800, Easing.CubicOut),
                LogoContainer.ScaleTo(1, 800, Easing.CubicOut)
            );
            
            // Subtle bounce effect for the logo
            await Task.Delay(200);
            await GateSaleLogo.ScaleTo(1.05, 300, Easing.CubicOut);
            await GateSaleLogo.ScaleTo(1.0, 200, Easing.CubicIn);
        }

        private async Task AnimateAppNameText()
        {
            // Animate app name after logo appears
            await Task.Delay(800);
            
            // Fade in app name with slide up effect
            await Task.WhenAll(
                AppNameContainer.FadeTo(1, 600, Easing.CubicOut),
                AppNameContainer.TranslateTo(0, 0, 600, Easing.CubicOut)
            );
            
            // Animate subtitle with slight delay
            await Task.Delay(200);
            await Task.WhenAll(
                Subtitle.FadeTo(1, 500, Easing.CubicOut),
                Subtitle.TranslateTo(0, 0, 500, Easing.CubicOut)
            );
        }

        private async Task AnimateDecorativeElements()
        {
            // Animate decorative elements with staggered timing
            await Task.Delay(1000);
            
            await DecorativeElements.FadeTo(1, 800, Easing.CubicOut);
        }

        private async Task AnimateParticles()
        {
            // Animate floating particles with random delays
            await Task.Delay(1200);
            
            await ParticlesContainer.FadeTo(1, 600, Easing.CubicOut);
            
            // Start floating animation for particles
            _ = Task.Run(async () =>
            {
                while (ParticlesContainer.Opacity > 0)
                {
                    // Animate particles with floating motion
                    var particleTasks = new[]
                    {
                        Particle1.TranslateTo(0, -10, 2000, Easing.SinInOut),
                        Particle2.TranslateTo(0, -15, 2500, Easing.SinInOut),
                        Particle3.TranslateTo(0, -8, 1800, Easing.SinInOut),
                        Particle4.TranslateTo(0, -12, 2200, Easing.SinInOut)
                    };
                    
                    await Task.WhenAll(particleTasks);
                    
                    // Reset positions
                    var resetTasks = new[]
                    {
                        Particle1.TranslateTo(0, 0, 0),
                        Particle2.TranslateTo(0, 0, 0),
                        Particle3.TranslateTo(0, 0, 0),
                        Particle4.TranslateTo(0, 0, 0)
                    };
                    
                    await Task.WhenAll(resetTasks);
                    await Task.Delay(1000);
                }
            });
        }

        private async Task AnimateProgressBar()
        {
            // Show loading progress bar
            await Task.Delay(1500);
            
            // Fade in loading container
            await LoadingContainer.FadeTo(1, 500, Easing.CubicOut);
            
            // Animate progress bar width
            var progressAnimation = new Animation(
                callback: (value) => ProgressBar.WidthRequest = value * 200,
                start: 0,
                end: 1,
                easing: Easing.CubicOut
            );
            
            progressAnimation.Commit(ProgressBar, "ProgressAnimation", 16, 2000);
            await Task.Delay(2000);
        }

        private async Task NavigateToMainPage()
        {
            try
            {
                // Fade out animation before navigation
                await Task.WhenAll(
                    MainContainer.FadeTo(0, 500, Easing.CubicIn),
                    LoadingContainer.FadeTo(0, 500, Easing.CubicIn),
                    DecorativeElements.FadeTo(0, 500, Easing.CubicIn),
                    ParticlesContainer.FadeTo(0, 500, Easing.CubicIn)
                );

                // Navigate to MainPage directly (avoiding Shell to resolve Android fragment conflict with BlazorWebView)
                await MainThread.InvokeOnMainThreadAsync(() => {
                    if (Application.Current != null)
                    {
                        Application.Current.MainPage = new MainPage();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                // Fallback navigation
                await MainThread.InvokeOnMainThreadAsync(() => {
                    if (Application.Current != null)
                    {
                        Application.Current.MainPage = new MainPage();
                    }
                });
            }
        }

        // Handle back button to prevent going back to splash
        protected override bool OnBackButtonPressed()
        {
            return true; // Consume the back button press
        }
    }
}