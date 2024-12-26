using System;
using System.Timers;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace AimTrainer
{
    class MainWindow : Window
    {
        [UI] private Label _label1 = null;
        [UI] private Label _label2 = null;
        [UI] private Button _button1 = null;

        [UI] private Box _startBox = null;

        [UI] private Fixed _mainBox = null;
        
        private Button _aimButton = null;

        private int _clickedCounter = 0;

        private readonly Timer _roundTimer = new Timer(60_000);

        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
            _mainBox = new Fixed();
            _mainBox.Visible = true;
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            _button1.Clicked += Button1_Clicked;

            _roundTimer.AutoReset = false;
            _roundTimer.Elapsed += RoundTimer_Elapsed;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            _label1.Visible = false;
            _label2.Visible = false;
            _button1.Visible = false;

            Remove(_startBox);
            Add(_mainBox);

            CreateNewAimBox();

            _roundTimer.Start();
        }
        
        private void AimButton_Clicked(object sender, EventArgs a)
        {
            _clickedCounter++;
            ClearCurrentAimBox();
            CreateNewAimBox();
        }
        
        private void RoundTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _roundTimer.Stop();
            this.Remove(_mainBox);
            
            ClearCurrentAimBox();
            
            var score = _clickedCounter;
            _label1.Text = $"You scored {score}!";
            _label1.Visible = true;
            _button1.Label = "Click here to play again!";
            _button1.Visible = true;

            _clickedCounter = 0;
            
            this.Add(_startBox);
        }

        private void CreateNewAimBox()
        {
            var randomSize = Random.Shared.Next(16, 77);

            _aimButton = new Button();
            _aimButton.Visible = true;
            _aimButton.SetSizeRequest(randomSize, randomSize);
            
            _aimButton.Clicked += AimButton_Clicked;

            var colour = GenerateRandomColor();
            SetButtonBackgroundColor(_aimButton, colour);

            var maxSize = this.Allocation.Size;

            var x = Random.Shared.Next(0, maxSize.Width - randomSize);
            var y = Random.Shared.Next(0, maxSize.Height - randomSize);

            _mainBox.Put(_aimButton, x, y);
        }
        
        private void ClearCurrentAimBox()
        {
            _aimButton.Clicked -= AimButton_Clicked;
            _mainBox.Remove(_aimButton);
        }

        // Generate a random color in hex format
        private static string GenerateRandomColor()
        {
            int r = Random.Shared.Next(0, 256); // Red component (0-255)
            int g = Random.Shared.Next(0, 256); // Green component (0-255)
            int b = Random.Shared.Next(0, 256); // Blue component (0-255)
            return $"#{r:X2}{g:X2}{b:X2}"; // Hexadecimal color format
        }

        // Set the button's background color using CSS
        private static void SetButtonBackgroundColor(Button button, string color)
        {
            var cssProvider = new CssProvider();
            cssProvider.LoadFromData($"button {{ background: {color}; border: none; }}");

            var styleContext = button.StyleContext;
            styleContext.AddProvider(cssProvider, StyleProviderPriority.Application);
        }
    }
}