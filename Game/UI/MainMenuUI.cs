﻿using K8055Velleman.Game.Saves;
using K8055Velleman.Lib.CustomControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace K8055Velleman.Game.UI
{
    internal class MainMenuUI : UIBase
    {

        private Control _mainMenu;
        private Panel _vellmanBoardStatusPanel;
        private Label _vellmanBoardStatusLabel;
        private BButton _playButton;
        private BButton _settingsButton;
        private BButton _quitButton;
        private BButton _changePlayer;
        private BButton _creditMenuButton;
        private Panel _scoreboard;

        internal override void OnCreate()
        {
            base.OnCreate();
            _mainMenu = new()
            {
                Location = new Point(0, 0),
                Width = GameWindow.Width,
                Height = GameWindow.Height,
                BackColor = Color.Black,
            };

            Label GameName = new()
            {
                Text = "Cosmic Siege: Alien Assault",
                Width = 750,
                Height = 150,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new(FontFamily.GenericSerif, 45f, FontStyle.Bold),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            GameName.Location = new(_mainMenu.Width/2 - GameName.Width/2, 200);

            Label PlayerName = new()
            {
                Text = SaveManager.CurrentPlayerData?.Name,
                Location = new Point(25,25),
                AutoSize = true,
                Font = new Font(UIManager.CustomFonts.Families[0], 15f, FontStyle.Bold),
                ForeColor= Color.WhiteSmoke,
            };

            _vellmanBoardStatusPanel = new()
            {
                Width = 150,
                Height = 30,
                BackColor = K8055.IsConnected ? Color.Green : Color.Red
            };
            _vellmanBoardStatusPanel.Location = new(_mainMenu.Right - 10 - _vellmanBoardStatusPanel.Width - RightOffeset, _mainMenu.Location.Y + 10);

            _vellmanBoardStatusLabel = new()
            {
                Height = _vellmanBoardStatusPanel.Height,
                Width = _vellmanBoardStatusPanel.Width,
                Text = K8055.IsConnected ? "Connected" : "Disconnected",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new(FontFamily.GenericSerif, 15f, FontStyle.Regular),
            };

            _playButton = new()
            {
                Height = 96,
                Width = 350,
                Text = K8055.IsConnected ? "Play (INP1)" : "Play",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 17f, FontStyle.Regular)
            };
            _playButton.Location = new(_mainMenu.Left + _mainMenu.Width / 2 - _playButton.Width/2, 450);
            _playButton.Click += (s, e) => { GameManager.Load(GameStatus.PreGame); };

            _settingsButton = new()
            {
                Height = 96,
                Width = 350,
                Text = K8055.IsConnected ? "Settings (INP2)" : "Settings",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 17f, FontStyle.Regular)
            };
            _settingsButton.Location = new(_mainMenu.Left + _mainMenu.Width / 2 - _settingsButton.Width / 2, 600);
            _settingsButton.Click += (s, e) => { UIManager.GetOrCreateUI<SettingsUI>().Show(_mainMenu); };

            _quitButton = new()
            {
                Height = 96,
                Width = 350,
                Text = K8055.IsConnected ? "Quit (INP5)" : "Quit",
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 17f, FontStyle.Regular)
            };
            _quitButton.Location = new(_mainMenu.Left + _mainMenu.Width / 2 - _quitButton.Width / 2, 750);
            _quitButton.Click += (s, e) => { GameWindow.Close(); };

            UpdateScoreboard();

            _creditMenuButton = new()
            {
                Text = K8055.IsConnected ? "Credit (INP4)" : "Credit",
                Width = 256,
                Height = 64,
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 17f, FontStyle.Regular)
            };
            _creditMenuButton.Location = new(_mainMenu.Width - _creditMenuButton.Width - 50, _mainMenu.Height - _creditMenuButton.Height - 50 );
            _creditMenuButton.Click += (s, e) => { ShowCreditMenu(); };

            _playButton.Focus();

            _vellmanBoardStatusPanel.Controls.Add(_vellmanBoardStatusLabel);
            _mainMenu.Controls.Add(_vellmanBoardStatusPanel);
            _mainMenu.Controls.Add(GameName);
            _mainMenu.Controls.Add(PlayerName);
            _mainMenu.Controls.Add(_playButton);
            _mainMenu.Controls.Add(_settingsButton);
            _mainMenu.Controls.Add(_quitButton);
            _mainMenu.Controls.Add(_creditMenuButton);
            GameWindow.Controls.Add(_mainMenu);
            Console.WriteLine("Main menu destroyed");
        }

        private void ShowCreditMenu()
        {
            UIManager.GetOrCreateUI<CreditUI>();
            UIManager.DestroyUI<MainMenuUI>();
        }

        internal override void OnDestroy()
        {	
            base.OnDestroy();
            Console.WriteLine("Main menu destroyed");
            AudioManager.StopSound(AudioFile.MouseOver);
            GameWindow.Controls.Remove(_mainMenu);
            _mainMenu.Dispose();
        }

        internal override void OnDigitalChannelsChange(K8055.DigitalChannel digitalChannel)
        {
            if(!_mainMenu.Enabled) return;
            if(digitalChannel == K8055.DigitalChannel.I1) _playButton.PerformClick();
            else if(digitalChannel == K8055.DigitalChannel.I2) _settingsButton.PerformClick();
            else if(digitalChannel == K8055.DigitalChannel.I3) _changePlayer.PerformClick();
            else if(digitalChannel == K8055.DigitalChannel.I4) _creditMenuButton.PerformClick();
            else if(digitalChannel == K8055.DigitalChannel.I5) _quitButton.PerformClick();
        }

        internal override void OnConnectionChange()
        {
            _vellmanBoardStatusPanel.BackColor = K8055.IsConnected ? Color.Green : Color.Red;
            _vellmanBoardStatusLabel.Text = K8055.IsConnected ? "Connected" : "Disconnected";
            _playButton.Text = K8055.IsConnected ? "Play (INP1)" : "Play";
            _settingsButton.Text = K8055.IsConnected ? "Settings (INP2)" : "Settings";
            _changePlayer.Text = K8055.IsConnected ? "Change Player (INP3)" : "Change Player";
            _changePlayer.Font = new Font(FontFamily.GenericSansSerif, K8055.IsConnected ? 15f : 17f, FontStyle.Regular);
            _creditMenuButton.Text = K8055.IsConnected ? "Credit (INP4)" : "Credit";
            _quitButton.Text = K8055.IsConnected ? "Quit (INP5)" : "Quit";
        }

        private void UpdateScoreboard()
        {
            if(_scoreboard != null)
            {
                _mainMenu.Controls.Remove(_scoreboard);
                _scoreboard.Dispose();
                _scoreboard = null;
            }
            _scoreboard = new()
            {
                Width = 300,
                Height = 500,
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.White,
            };
            _scoreboard.Location = new(_mainMenu.Width / 10 * 9 - _scoreboard.Width / 2, _mainMenu.Height / 2 - _scoreboard.Height / 2);

            Label label = new()
            {
                Width = _scoreboard.Width,
                Height = 50,
                Text = "Scoreboard",
                Location = new(0,0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(UIManager.CustomFonts.Families[0], 20f, FontStyle.Bold),
                ForeColor = Color.White,
            };

            Panel PlayerListScroll = new()
            {
                Width = _scoreboard.Width - 50,
                Height = _scoreboard.Height - 100 - label.Height,
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.White,
                AutoScroll = true,
                Location = new(25, label.Height),
            };

            _changePlayer = new()
            {
                Width = _scoreboard.Width - 50,
                Height = 50,
                Location = new(300, 675),
                Text = K8055.IsConnected ? "Change Player (INP3)" : "Change Player",
                Font = new Font(FontFamily.GenericSansSerif, K8055.IsConnected ? 15f : 17f, FontStyle.Regular),
            };
            _changePlayer.Location = new(25, _scoreboard.Height - _changePlayer.Height - 25);
            _changePlayer.Click += (s, e) => { GameManager.Load(GameStatus.PlayerSelector); };

            int x = 0;
            int offsetSpace = 20;
            List<PlayerData> playerDatas = new(SaveManager.s_playersData);
            playerDatas.Sort(delegate (PlayerData x, PlayerData y)
            {
                return y.HigestScore.CompareTo(x.HigestScore);
            });
            foreach (PlayerData playerData in playerDatas)
            {
                Panel Player = new()
                {
                    Width = PlayerListScroll.Width - offsetSpace * 2,
                    Height = 50,
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.White,
                    BackColor = (playerData == SaveManager.CurrentPlayerData) ? Color.LightBlue : Color.Transparent,
                };
                Player.Location = new(offsetSpace, Player.Height * x + offsetSpace * (x + 1));

                Label name = new()
                {
                    Width = Player.Width / 3 * 2,
                    Height = Player.Height,
                    Text = playerData.Name,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font(UIManager.CustomFonts.Families[0], 15f, FontStyle.Regular),
                    ForeColor = Color.White,
                };

                Label Score = new()
                {
                    Width = Player.Width / 3,
                    Height = Player.Height,
                    Text = $"{playerData.HigestScore} 🌟",
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font(UIManager.CustomFonts.Families[0], 15f, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(name.Width, 0),
                };

                Player.Controls.Add(name);
                Player.Controls.Add(Score);
                PlayerListScroll.Controls.Add(Player);	
                x++;
            }
            _scoreboard.Controls.Add(label);
            _scoreboard.Controls.Add(PlayerListScroll);
            _scoreboard.Controls.Add(_changePlayer);

            _mainMenu.Controls.Add(_scoreboard);
        }
    }
}
