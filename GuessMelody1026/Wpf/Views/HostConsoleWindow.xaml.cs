using System;
using System.Windows;
using GuessMelody.Wpf.ViewModels;

namespace GuessMelody.Wpf.Views
{
    public partial class HostConsoleWindow : Window
    {
        public HostConsoleViewModel ViewModel { get; }

        public HostConsoleWindow(HostConsoleViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;
            DataContext = vm;
        }

        private void Play_Click(object s, RoutedEventArgs e) => ViewModel.Play();
        private void Pause_Click(object s, RoutedEventArgs e) => ViewModel.Pause();
        private void Stop_Click(object s, RoutedEventArgs e) => ViewModel.Stop();

        private void Back10_Click(object s, RoutedEventArgs e) => ViewModel.SeekRelative(-10);
        private void Fwd10_Click(object s, RoutedEventArgs e) => ViewModel.SeekRelative(+10);

        private void HostYes_Click(object s, RoutedEventArgs e) => ViewModel.HostSaysYes(2);
        private void HostNo_Click(object s, RoutedEventArgs e) => ViewModel.HostSaysNo();
        private void HostNoOne_Click(object s, RoutedEventArgs e) => ViewModel.HostSaysNoOne();

        private void StartCat1_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(0);
        private void StartCat2_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(1);
        private void StartCat3_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(2);
        private void StartCat4_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(3);
        private void StartCat5_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(4);
        private void StartCat6_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(5);
        private void StartCat7_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(6);
        private void StartCat8_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(7);
        private void StartCat9_Click(object s, RoutedEventArgs e) => ViewModel.StartRound(8);

        private void ResetRound_Click(object s, RoutedEventArgs e)
        {
            ViewModel.Stop();
            ViewModel.HostSaysNoOne();
        }

        private void ResetScores_Click(object s, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сбросить очки всех игроков?",
                    "Подтверждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            ViewModel.ResetScores();
        }
    }
}