using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace FlooxOC
{
    public class MusicPlayerApp : Panel
    {
        private ListBox playlistBox;
        private Button btnPlay, btnPause, btnStop, btnAdd, btnRemove, btnClear;
        private Label lblCurrentSong, lblStatus;
        private Timer progressTimer;
        private ProgressBar progressBar;
        private SoundPlayer player;
        private string currentFile = "";
        private List<string> playlist = new List<string>();
        private bool isPlaying = false;

        public MusicPlayerApp()
        {
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(10);

            player = new SoundPlayer();
            progressTimer = new Timer();
            progressTimer.Interval = 100;
            progressTimer.Tick += ProgressTimer_Tick;

            InitializeComponents();
            LoadPlaylist();
        }

        private void InitializeComponents()
        {
            int y = 10;

            // Заголовок
            Label title = new Label();
            title.Text = "🎵 Музыкальный плеер";
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.Location = new Point(10, y);
            title.Size = new Size(300, 30);
            this.Controls.Add(title);
            y += 40;

            // Текущий трек
            lblCurrentSong = new Label();
            lblCurrentSong.Text = "Нет трека";
            lblCurrentSong.Font = new Font("Segoe UI", 10);
            lblCurrentSong.Location = new Point(10, y);
            lblCurrentSong.Size = new Size(400, 25);
            lblCurrentSong.ForeColor = Color.DarkBlue;
            this.Controls.Add(lblCurrentSong);
            y += 30;

            // Прогресс
            progressBar = new ProgressBar();
            progressBar.Location = new Point(10, y);
            progressBar.Size = new Size(400, 20);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            this.Controls.Add(progressBar);
            y += 30;

            // Кнопки управления
            btnPlay = new Button();
            btnPlay.Text = "▶️";
            btnPlay.Size = new Size(50, 40);
            btnPlay.Location = new Point(10, y);
            btnPlay.Font = new Font("Segoe UI", 14);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.BackColor = Color.FromArgb(0, 120, 215);
            btnPlay.ForeColor = Color.White;
            btnPlay.Click += PlaySong;
            this.Controls.Add(btnPlay);

            btnPause = new Button();
            btnPause.Text = "⏸️";
            btnPause.Size = new Size(50, 40);
            btnPause.Location = new Point(65, y);
            btnPause.Font = new Font("Segoe UI", 14);
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.BackColor = Color.FromArgb(255, 180, 0);
            btnPause.ForeColor = Color.Black;
            btnPause.Click += PauseSong;
            this.Controls.Add(btnPause);

            btnStop = new Button();
            btnStop.Text = "⏹️";
            btnStop.Size = new Size(50, 40);
            btnStop.Location = new Point(120, y);
            btnStop.Font = new Font("Segoe UI", 14);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.BackColor = Color.FromArgb(200, 50, 50);
            btnStop.ForeColor = Color.White;
            btnStop.Click += StopSong;
            this.Controls.Add(btnStop);

            y += 50;

            // Кнопки управления плейлистом
            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Size = new Size(90, 30);
            btnAdd.Location = new Point(10, y);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.BackColor = Color.FromArgb(0, 150, 80);
            btnAdd.ForeColor = Color.White;
            btnAdd.Click += AddSong;
            this.Controls.Add(btnAdd);

            btnRemove = new Button();
            btnRemove.Text = "❌ Удалить";
            btnRemove.Size = new Size(90, 30);
            btnRemove.Location = new Point(105, y);
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.BackColor = Color.FromArgb(200, 80, 80);
            btnRemove.ForeColor = Color.White;
            btnRemove.Click += RemoveSong;
            this.Controls.Add(btnRemove);

            btnClear = new Button();
            btnClear.Text = "🗑️ Очистить";
            btnClear.Size = new Size(90, 30);
            btnClear.Location = new Point(200, y);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.BackColor = Color.FromArgb(150, 150, 150);
            btnClear.ForeColor = Color.White;
            btnClear.Click += ClearPlaylist;
            this.Controls.Add(btnClear);

            y += 40;

            // Статус
            lblStatus = new Label();
            lblStatus.Text = "Готово";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(10, y);
            lblStatus.Size = new Size(400, 20);
            lblStatus.ForeColor = Color.DarkGreen;
            this.Controls.Add(lblStatus);

            y += 30;

            // Плейлист
            Label lblPlaylist = new Label();
            lblPlaylist.Text = "📋 Плейлист:";
            lblPlaylist.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPlaylist.Location = new Point(10, y);
            lblPlaylist.Size = new Size(200, 25);
            this.Controls.Add(lblPlaylist);

            y += 30;

            playlistBox = new ListBox();
            playlistBox.Location = new Point(10, y);
            playlistBox.Size = new Size(400, 200);
            playlistBox.Font = new Font("Segoe UI", 10);
            playlistBox.BackColor = Color.White;
            playlistBox.DoubleClick += Playlist_DoubleClick;
            this.Controls.Add(playlistBox);
        }

        private void LoadPlaylist()
        {
            string playlistFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Floox OC. Home version", "playlist.txt");

            if (File.Exists(playlistFile))
            {
                try
                {
                    string[] songs = File.ReadAllLines(playlistFile);
                    foreach (string song in songs)
                    {
                        if (File.Exists(song))
                        {
                            playlist.Add(song);
                            playlistBox.Items.Add(Path.GetFileName(song));
                        }
                    }
                }
                catch { }
            }
        }

        private void SavePlaylist()
        {
            try
            {
                string playlistFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Floox OC. Home version", "playlist.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(playlistFile));
                File.WriteAllLines(playlistFile, playlist);
            }
            catch { }
        }

        private void AddSong(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Аудио файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        if (!playlist.Contains(file))
                        {
                            playlist.Add(file);
                            playlistBox.Items.Add(Path.GetFileName(file));
                        }
                    }
                    SavePlaylist();
                    lblStatus.Text = $"Добавлено {ofd.FileNames.Length} треков";
                    lblStatus.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void RemoveSong(object sender, EventArgs e)
        {
            if (playlistBox.SelectedIndex >= 0)
            {
                int index = playlistBox.SelectedIndex;
                playlist.RemoveAt(index);
                playlistBox.Items.RemoveAt(index);
                SavePlaylist();
                lblStatus.Text = "Трек удалён";
                lblStatus.ForeColor = Color.DarkGreen;

                if (currentFile == playlistBox.SelectedItem?.ToString())
                {
                    StopSong(null, null);
                }
            }
        }

        private void ClearPlaylist(object sender, EventArgs e)
        {
            if (playlist.Count > 0)
            {
                DialogResult result = MessageBox.Show("Очистить весь плейлист?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    playlist.Clear();
                    playlistBox.Items.Clear();
                    SavePlaylist();
                    StopSong(null, null);
                    lblStatus.Text = "Плейлист очищен";
                    lblStatus.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void Playlist_DoubleClick(object sender, EventArgs e)
        {
            if (playlistBox.SelectedIndex >= 0)
            {
                PlaySongFromIndex(playlistBox.SelectedIndex);
            }
        }

        private void PlaySongFromIndex(int index)
        {
            if (index >= 0 && index < playlist.Count)
            {
                currentFile = playlist[index];
                lblCurrentSong.Text = Path.GetFileName(currentFile);

                try
                {
                    player.SoundLocation = currentFile;
                    player.Load();
                    player.Play();
                    isPlaying = true;
                    progressTimer.Start();
                    lblStatus.Text = "▶️ Воспроизведение";
                    lblStatus.ForeColor = Color.DarkGreen;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Ошибка воспроизведения";
                    lblStatus.ForeColor = Color.Red;
                }
            }
        }

        private void PlaySong(object sender, EventArgs e)
        {
            if (playlistBox.SelectedIndex >= 0)
            {
                PlaySongFromIndex(playlistBox.SelectedIndex);
            }
            else if (playlist.Count > 0)
            {
                PlaySongFromIndex(0);
            }
            else
            {
                MessageBox.Show("Плейлист пуст! Добавьте треки.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PauseSong(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                player.Stop();
                isPlaying = false;
                progressTimer.Stop();
                lblStatus.Text = "⏸️ На паузе";
                lblStatus.ForeColor = Color.DarkOrange;
            }
        }

        private void StopSong(object sender, EventArgs e)
        {
            player.Stop();
            isPlaying = false;
            progressTimer.Stop();
            progressBar.Value = 0;
            lblCurrentSong.Text = "Остановлено";
            lblStatus.Text = "⏹️ Остановлено";
            lblStatus.ForeColor = Color.DarkRed;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                // Имитация прогресса (WAV не поддерживает позицию)
                progressBar.Value = (progressBar.Value + 1) % 100;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                progressTimer?.Stop();
                progressTimer?.Dispose();
                player?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}