using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FlooxOC
{
    public class MusicPlayerApp : Panel
    {
        private ListBox playlistBox;
        private Button btnPlay, btnPause, btnStop, btnAdd, btnRemove, btnClear;
        private Button btnPrev, btnNext;
        private Label lblCurrentSong, lblStatus, lblTime;
        private Timer progressTimer;
        private ProgressBar progressBar;
        private List<string> playlist = new List<string>();
        private bool isPlaying = false;
        private bool isPaused = false;
        private int currentIndex = -1;
        private string tempWavFile = "";

        // NAudio
        private WaveOutEvent waveOut;
        private AudioFileReader audioFileReader;
        private VolumeSampleProvider volumeProvider;

        // Громкость
        private TrackBar tbVolume;
        private NumericUpDown nudVolume;
        private Button btnMute;
        private float volume = 1.0f;

        public MusicPlayerApp()
        {
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(15);

            progressTimer = new Timer();
            progressTimer.Interval = 100;
            progressTimer.Tick += ProgressTimer_Tick;

            InitializeComponents();
            LoadPlaylist();
            ApplyContrastColors(this, this.BackColor);
        }

        private void InitializeComponents()
        {
            // ===== ОСНОВНАЯ ПАНЕЛЬ (левая часть) =====
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Left;
            mainPanel.Width = (int)(this.Width * 0.80);
            mainPanel.BackColor = Color.Transparent;
            this.Controls.Add(mainPanel);

            int y = 15;

            Label title = new Label();
            title.Text = "🎵 Музыкальный плеер";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(15, y);
            title.Size = new Size(350, 35);
            mainPanel.Controls.Add(title);
            y += 50;

            lblCurrentSong = new Label();
            lblCurrentSong.Text = "Нет трека";
            lblCurrentSong.Font = new Font("Segoe UI", 12);
            lblCurrentSong.Location = new Point(15, y);
            lblCurrentSong.Size = new Size(450, 30);
            lblCurrentSong.ForeColor = Color.DarkBlue;
            mainPanel.Controls.Add(lblCurrentSong);
            y += 40;

            progressBar = new ProgressBar();
            progressBar.Location = new Point(15, y);
            progressBar.Size = new Size(450, 25);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            mainPanel.Controls.Add(progressBar);

            lblTime = new Label();
            lblTime.Text = "00:00 / 00:00";
            lblTime.Font = new Font("Segoe UI", 10);
            lblTime.Location = new Point(370, y + 5);
            lblTime.Size = new Size(95, 20);
            lblTime.TextAlign = ContentAlignment.MiddleRight;
            mainPanel.Controls.Add(lblTime);

            y += 40;

            int btnX = 15;

            btnPrev = new Button();
            btnPrev.Text = "⏮️";
            btnPrev.Size = new Size(50, 45);
            btnPrev.Location = new Point(btnX, y);
            btnPrev.Font = new Font("Segoe UI", 14);
            btnPrev.FlatStyle = FlatStyle.Flat;
            btnPrev.BackColor = Color.FromArgb(100, 100, 100);
            btnPrev.ForeColor = Color.White;
            btnPrev.Click += (s, e) => PlayPreviousSong();
            mainPanel.Controls.Add(btnPrev);
            btnX += 55;

            btnPlay = new Button();
            btnPlay.Text = "▶️";
            btnPlay.Size = new Size(55, 45);
            btnPlay.Location = new Point(btnX, y);
            btnPlay.Font = new Font("Segoe UI", 16);
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.BackColor = Color.FromArgb(0, 120, 215);
            btnPlay.ForeColor = Color.White;
            btnPlay.Click += PlaySong;
            mainPanel.Controls.Add(btnPlay);
            btnX += 60;

            btnPause = new Button();
            btnPause.Text = "⏸️";
            btnPause.Size = new Size(50, 45);
            btnPause.Location = new Point(btnX, y);
            btnPause.Font = new Font("Segoe UI", 14);
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.BackColor = Color.FromArgb(255, 180, 0);
            btnPause.ForeColor = Color.Black;
            btnPause.Click += PauseSong;
            mainPanel.Controls.Add(btnPause);
            btnX += 55;

            btnStop = new Button();
            btnStop.Text = "⏹️";
            btnStop.Size = new Size(50, 45);
            btnStop.Location = new Point(btnX, y);
            btnStop.Font = new Font("Segoe UI", 14);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.BackColor = Color.FromArgb(200, 50, 50);
            btnStop.ForeColor = Color.White;
            btnStop.Click += StopSong;
            mainPanel.Controls.Add(btnStop);
            btnX += 55;

            btnNext = new Button();
            btnNext.Text = "⏭️";
            btnNext.Size = new Size(50, 45);
            btnNext.Location = new Point(btnX, y);
            btnNext.Font = new Font("Segoe UI", 14);
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.BackColor = Color.FromArgb(100, 100, 100);
            btnNext.ForeColor = Color.White;
            btnNext.Click += (s, e) => PlayNextSong();
            mainPanel.Controls.Add(btnNext);

            y += 55;

            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Size = new Size(100, 35);
            btnAdd.Location = new Point(15, y);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.BackColor = Color.FromArgb(0, 150, 80);
            btnAdd.ForeColor = Color.White;
            btnAdd.Font = new Font("Segoe UI", 10);
            btnAdd.Click += AddSong;
            mainPanel.Controls.Add(btnAdd);

            btnRemove = new Button();
            btnRemove.Text = "❌ Удалить";
            btnRemove.Size = new Size(100, 35);
            btnRemove.Location = new Point(120, y);
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.BackColor = Color.FromArgb(200, 80, 80);
            btnRemove.ForeColor = Color.White;
            btnRemove.Font = new Font("Segoe UI", 10);
            btnRemove.Click += RemoveSong;
            mainPanel.Controls.Add(btnRemove);

            btnClear = new Button();
            btnClear.Text = "🗑️ Очистить";
            btnClear.Size = new Size(100, 35);
            btnClear.Location = new Point(225, y);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.BackColor = Color.FromArgb(150, 150, 150);
            btnClear.ForeColor = Color.White;
            btnClear.Font = new Font("Segoe UI", 10);
            btnClear.Click += ClearPlaylist;
            mainPanel.Controls.Add(btnClear);

            y += 50;

            lblStatus = new Label();
            lblStatus.Text = "Готово";
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.Location = new Point(15, y);
            lblStatus.Size = new Size(450, 25);
            lblStatus.ForeColor = Color.DarkGreen;
            mainPanel.Controls.Add(lblStatus);

            y += 40;

            Label lblPlaylist = new Label();
            lblPlaylist.Text = "📋 Плейлист:";
            lblPlaylist.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblPlaylist.Location = new Point(15, y);
            lblPlaylist.Size = new Size(200, 30);
            mainPanel.Controls.Add(lblPlaylist);

            y += 40;

            playlistBox = new ListBox();
            playlistBox.Location = new Point(15, y);
            playlistBox.Size = new Size(450, 280);
            playlistBox.Font = new Font("Segoe UI", 11);
            playlistBox.BackColor = Color.White;
            playlistBox.SelectedIndexChanged += Playlist_SelectedIndexChanged;  // ← ВЫБОР ТРЕКА
            playlistBox.DoubleClick += Playlist_DoubleClick;
            mainPanel.Controls.Add(playlistBox);

            // ===== ПАНЕЛЬ ГРОМКОСТИ (правая часть) =====
            Panel volumePanel = new Panel();
            volumePanel.Dock = DockStyle.Right;
            volumePanel.Width = 100;
            volumePanel.BackColor = Color.FromArgb(210, 210, 210);
            volumePanel.Padding = new Padding(10);
            this.Controls.Add(volumePanel);

            BuildVolumePanel(volumePanel);
        }

        private void BuildVolumePanel(Panel parent)
        {
            int y = 15;

            Label title = new Label();
            title.Text = "🔊 Громкость";
            title.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            title.Location = new Point(10, y);
            title.Size = new Size(80, 25);
            title.TextAlign = ContentAlignment.MiddleCenter;
            parent.Controls.Add(title);
            y += 35;

            nudVolume = new NumericUpDown();
            nudVolume.Location = new Point(15, y);
            nudVolume.Size = new Size(60, 25);
            nudVolume.Minimum = 0;
            nudVolume.Maximum = 100;
            nudVolume.Value = 100;
            nudVolume.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            nudVolume.TextAlign = HorizontalAlignment.Center;
            nudVolume.ValueChanged += (s, e) =>
            {
                volume = (float)nudVolume.Value / 100f;
                if (volumeProvider != null)
                    volumeProvider.Volume = volume;
                tbVolume.Value = (int)nudVolume.Value;
                UpdateMuteButton();
            };
            parent.Controls.Add(nudVolume);
            y += 40;

            tbVolume = new TrackBar();
            tbVolume.Location = new Point(25, y);
            tbVolume.Size = new Size(40, 180);
            tbVolume.Minimum = 0;
            tbVolume.Maximum = 100;
            tbVolume.Value = 100;
            tbVolume.TickStyle = TickStyle.Both;
            tbVolume.Orientation = Orientation.Vertical;
            tbVolume.ValueChanged += (s, e) =>
            {
                volume = tbVolume.Value / 100f;
                if (volumeProvider != null)
                    volumeProvider.Volume = volume;
                nudVolume.Value = tbVolume.Value;
                UpdateMuteButton();
            };
            parent.Controls.Add(tbVolume);
            y += 190;

            Label lblPercent = new Label();
            lblPercent.Text = "%";
            lblPercent.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPercent.Location = new Point(25, y);
            lblPercent.Size = new Size(40, 25);
            lblPercent.TextAlign = ContentAlignment.MiddleCenter;
            parent.Controls.Add(lblPercent);
            y += 30;

            // ===== КНОПКА MUTE (переключает 0 ↔ 100) =====
            btnMute = new Button();
            btnMute.Text = "🔊";
            btnMute.Size = new Size(60, 40);
            btnMute.Location = new Point(10, y);
            btnMute.FlatStyle = FlatStyle.Flat;
            btnMute.BackColor = Color.FromArgb(0, 120, 215);
            btnMute.ForeColor = Color.White;
            btnMute.Font = new Font("Segoe UI", 16);
            btnMute.Cursor = Cursors.Hand;
            btnMute.Click += (s, e) =>
            {
                if (tbVolume.Value == 0)
                {
                    // Если громкость = 0, ставим 100
                    tbVolume.Value = 100;
                    nudVolume.Value = 100;
                    volume = 1.0f;
                    if (volumeProvider != null)
                        volumeProvider.Volume = 1.0f;
                }
                else
                {
                    // Иначе ставим 0
                    tbVolume.Value = 0;
                    nudVolume.Value = 0;
                    volume = 0.0f;
                    if (volumeProvider != null)
                        volumeProvider.Volume = 0.0f;
                }
                UpdateMuteButton();
            };
            parent.Controls.Add(btnMute);
        }

        private void UpdateMuteButton()
        {
            if (btnMute != null)
            {
                if (tbVolume.Value == 0)
                {
                    btnMute.Text = "🔇";
                    btnMute.BackColor = Color.FromArgb(200, 50, 50);
                }
                else
                {
                    btnMute.Text = "🔊";
                    btnMute.BackColor = Color.FromArgb(0, 120, 215);
                }
            }
        }

        // ====== ВЫБОР ТРЕКА ИЗ СПИСКА ======
        private void Playlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playlistBox.SelectedIndex >= 0)
            {
                // Просто выделяем трек в списке
                // Воспроизведение начинается по двойному клику или кнопке Play
            }
        }

        // ====== КОНВЕРТАЦИЯ В WAV ======
        private string ConvertToWav(string inputFile)
        {
            try
            {
                string ext = Path.GetExtension(inputFile).ToLower();

                if (ext == ".wav")
                {
                    return inputFile;
                }

                string tempFile = Path.GetTempFileName() + ".wav";

                using (var reader = new Mp3FileReader(inputFile))
                {
                    WaveFileWriter.CreateWaveFile(tempFile, reader);
                }

                tempWavFile = tempFile;
                return tempFile;
            }
            catch
            {
                return inputFile;
            }
        }

        // ====== АВТОМАТИЧЕСКИЙ ВЫБОР ЦВЕТА ======
        private Color GetContrastColor(Color backgroundColor)
        {
            double brightness = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
            return brightness < 0.5 ? Color.White : Color.Black;
        }

        private void ApplyContrastColors(Control parentControl, Color bgColor)
        {
            Color contrastColor = GetContrastColor(bgColor);

            foreach (Control child in parentControl.Controls)
            {
                if (child is Label label)
                {
                    label.ForeColor = contrastColor;
                }
                else if (child is Button button)
                {
                    button.ForeColor = GetContrastColor(button.BackColor);
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.ForeColor = contrastColor;
                }
                else if (child is ListBox listBox)
                {
                    listBox.ForeColor = contrastColor;
                    listBox.BackColor = contrastColor == Color.White ? Color.FromArgb(64, 64, 64) : Color.White;
                }
                else if (child is Panel panelControl)
                {
                    ApplyContrastColors(panelControl, bgColor);
                }
                else if (child is TextBox textBox)
                {
                    textBox.BackColor = contrastColor == Color.White ? Color.FromArgb(64, 64, 64) : Color.White;
                    textBox.ForeColor = GetContrastColor(textBox.BackColor);
                }
                else if (child is TrackBar trackBar)
                {
                    // TrackBar не требует изменения цвета
                }
                else if (child is NumericUpDown numericUpDown)
                {
                    numericUpDown.BackColor = contrastColor == Color.White ? Color.FromArgb(64, 64, 64) : Color.White;
                    numericUpDown.ForeColor = contrastColor;
                }
                else
                {
                    ApplyContrastColors(child, bgColor);
                }
            }
        }

        // ===== НАВИГАЦИЯ =====
        private void PlayPreviousSong()
        {
            if (playlist.Count == 0)
            {
                MessageBox.Show("Плейлист пуст!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StopSong(null, null);

            int newIndex = currentIndex - 1;
            if (newIndex < 0)
                newIndex = playlist.Count - 1;

            PlaySongFromIndex(newIndex);
        }

        private void PlayNextSong()
        {
            if (playlist.Count == 0)
            {
                MessageBox.Show("Плейлист пуст!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StopSong(null, null);

            int newIndex = currentIndex + 1;
            if (newIndex >= playlist.Count)
                newIndex = 0;

            PlaySongFromIndex(newIndex);
        }

        // ===== ПЛЕЙЛИСТ =====
        private void LoadPlaylist()
        {
            string playlistFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyOS95", "playlist.txt");

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
                        else
                        {
                            playlistBox.Items.Add(Path.GetFileName(song) + " (не найден)");
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
                    "MyOS95", "playlist.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(playlistFile));
                File.WriteAllLines(playlistFile, playlist);
            }
            catch { }
        }

        private void AddSong(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Аудио файлы (*.mp3;*.wav;*.ogg;*.flac;*.aac;*.wma)|*.mp3;*.wav;*.ogg;*.flac;*.aac;*.wma|MP3 (*.mp3)|*.mp3|WAV (*.wav)|*.wav|Все файлы (*.*)|*.*";
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

                if (currentIndex == index)
                {
                    StopSong(null, null);
                    currentIndex = -1;
                }
                else if (currentIndex > index)
                {
                    currentIndex--;
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
                    StopSong(null, null);
                    playlist.Clear();
                    playlistBox.Items.Clear();
                    SavePlaylist();
                    currentIndex = -1;
                    lblStatus.Text = "Плейлист очищен";
                    lblStatus.ForeColor = Color.DarkGreen;
                    lblCurrentSong.Text = "Нет трека";
                    lblTime.Text = "00:00 / 00:00";
                    progressBar.Value = 0;
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

        // ===== ОСНОВНОЙ МЕТОД ВОСПРОИЗВЕДЕНИЯ =====
        private void PlaySongFromIndex(int index)
        {
            if (index < 0 || index >= playlist.Count) return;

            try
            {
                StopSong(null, null);

                Application.DoEvents();
                System.Threading.Thread.Sleep(50);

                currentIndex = index;
                string filePath = playlist[index];

                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Файл не найден:\n{filePath}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    playlist.RemoveAt(index);
                    playlistBox.Items.RemoveAt(index);
                    currentIndex = -1;
                    return;
                }

                try
                {
                    audioFileReader = new AudioFileReader(filePath);
                }
                catch
                {
                    string wavFile = ConvertToWav(filePath);
                    audioFileReader = new AudioFileReader(wavFile);
                }

                volumeProvider = new VolumeSampleProvider(audioFileReader.ToSampleProvider());
                volumeProvider.Volume = volume;

                waveOut = new WaveOutEvent();
                waveOut.DesiredLatency = 100;
                waveOut.Init(volumeProvider);

                isPlaying = true;
                isPaused = false;
                btnPlay.Text = "▶️";

                lblCurrentSong.Text = Path.GetFileName(filePath);
                lblStatus.Text = "▶️ Воспроизведение";
                lblStatus.ForeColor = Color.DarkGreen;

                UpdateTimeDisplay();
                progressTimer.Start();
                waveOut.Play();

                waveOut.PlaybackStopped += (s, args) =>
                {
                    if (args.Exception != null)
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            MessageBox.Show($"Ошибка воспроизведения: {args.Exception.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            StopSong(null, null);
                        }));
                    }
                    else if (isPlaying && !isPaused)
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            if (currentIndex < playlist.Count - 1)
                            {
                                PlaySongFromIndex(currentIndex + 1);
                            }
                            else
                            {
                                StopSong(null, null);
                            }
                        }));
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка воспроизведения";
                lblStatus.ForeColor = Color.Red;
                StopSong(null, null);
            }
        }

        private void PlaySong(object sender, EventArgs e)
        {
            if (playlist.Count == 0)
            {
                MessageBox.Show("Плейлист пуст! Добавьте треки.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (isPaused && waveOut != null && audioFileReader != null)
            {
                try
                {
                    waveOut.Play();
                    isPaused = false;
                    isPlaying = true;
                    progressTimer.Start();
                    btnPlay.Text = "▶️";
                    lblStatus.Text = "▶️ Воспроизведение";
                    lblStatus.ForeColor = Color.DarkGreen;
                    return;
                }
                catch
                {
                    PlaySongFromIndex(currentIndex >= 0 ? currentIndex : 0);
                    return;
                }
            }

            // Если есть выделенный трек в списке — начинаем с него
            if (playlistBox.SelectedIndex >= 0 && playlistBox.SelectedIndex != currentIndex)
            {
                PlaySongFromIndex(playlistBox.SelectedIndex);
                return;
            }

            if (currentIndex >= 0)
            {
                PlaySongFromIndex(currentIndex);
            }
            else if (playlistBox.SelectedIndex >= 0)
            {
                PlaySongFromIndex(playlistBox.SelectedIndex);
            }
            else
            {
                PlaySongFromIndex(0);
            }
        }

        private void PauseSong(object sender, EventArgs e)
        {
            if (waveOut != null && isPlaying && !isPaused)
            {
                try
                {
                    waveOut.Pause();
                    isPaused = true;
                    progressTimer.Stop();
                    btnPlay.Text = "▶️";
                    lblStatus.Text = "⏸️ На паузе";
                    lblStatus.ForeColor = Color.DarkOrange;
                }
                catch { }
            }
        }

        private void StopSong(object sender, EventArgs e)
        {
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
            }
            catch { }

            try
            {
                if (audioFileReader != null)
                {
                    audioFileReader.Dispose();
                    audioFileReader = null;
                }
            }
            catch { }

            try
            {
                if (volumeProvider != null)
                {
                    volumeProvider = null;
                }
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(tempWavFile) && File.Exists(tempWavFile))
                {
                    File.Delete(tempWavFile);
                    tempWavFile = "";
                }
            }
            catch { }

            isPlaying = false;
            isPaused = false;
            progressTimer.Stop();
            progressBar.Value = 0;

            if (!string.IsNullOrEmpty(lblCurrentSong.Text) && lblCurrentSong.Text != "Нет трека")
            {
                lblCurrentSong.Text = "Остановлено";
            }
            lblStatus.Text = "⏹️ Остановлено";
            lblStatus.ForeColor = Color.DarkRed;
            lblTime.Text = "00:00 / 00:00";
            btnPlay.Text = "▶️";

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (audioFileReader != null && waveOut != null && isPlaying)
            {
                try
                {
                    double totalSeconds = audioFileReader.TotalTime.TotalSeconds;
                    double currentSeconds = audioFileReader.CurrentTime.TotalSeconds;

                    if (totalSeconds > 0)
                    {
                        int progress = (int)((currentSeconds / totalSeconds) * 100);
                        progressBar.Value = Math.Min(progress, 100);
                    }

                    UpdateTimeDisplay();
                }
                catch { }
            }
        }

        private void UpdateTimeDisplay()
        {
            if (audioFileReader != null)
            {
                try
                {
                    string current = audioFileReader.CurrentTime.ToString(@"mm\:ss");
                    string total = audioFileReader.TotalTime.ToString(@"mm\:ss");
                    lblTime.Text = $"{current} / {total}";
                }
                catch
                {
                    lblTime.Text = "00:00 / 00:00";
                }
            }
            else
            {
                lblTime.Text = "00:00 / 00:00";
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel && panel.Dock == DockStyle.Left)
                {
                    panel.Width = (int)(this.Width * 0.80);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                progressTimer?.Stop();
                progressTimer?.Dispose();

                try
                {
                    if (waveOut != null)
                    {
                        waveOut.Stop();
                        waveOut.Dispose();
                    }
                }
                catch { }

                try
                {
                    if (audioFileReader != null)
                    {
                        audioFileReader.Dispose();
                    }
                }
                catch { }

                try
                {
                    if (!string.IsNullOrEmpty(tempWavFile) && File.Exists(tempWavFile))
                    {
                        File.Delete(tempWavFile);
                    }
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}