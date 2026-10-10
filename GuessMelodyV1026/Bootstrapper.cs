using GuessMelody.Audio;
using GuessMelody.Buttons;
using GuessMelody.Core;
using GuessMelody.Services;
using GuessMelody.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GuessMelody
{
    public static class Bootstrapper
    {
        public static ServiceProvider Build()
        {
            var services = new ServiceCollection();

            // -------- Core --------
            services.AddSingleton<LogService>();
            services.AddSingleton<GameEngine>();

            // -------- Services --------
            services.AddSingleton<DialogService>();
            services.AddSingleton<RecentFilesService>();

            // -------- Buttons --------
            services.AddSingleton<ButtonService>(sp =>
                new ButtonService(sp.GetRequiredService<LogService>()));

            // -------- Audio --------
            services.AddSingleton<AudioEngine>();
            services.AddSingleton<PreviewEngine>();
            services.AddSingleton<BuzzerPlayer>();

            // -------- ViewModels --------
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<GameTabViewModel>();
            services.AddSingleton<SettingsTabViewModel>();
            services.AddSingleton<ButtonsTabViewModel>();
            services.AddSingleton<FoldersTabViewModel>();
            services.AddSingleton<LogTabViewModel>();
            services.AddSingleton<AnswerServerTabViewModel>();

            return services.BuildServiceProvider();
        }
    }
}