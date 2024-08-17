using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;


namespace Code
{
    public class MainMenu
    {
        public enum MenuState
        {
            Main,
            LevelSelect,
            Victory
        }

        private double _lastClickTime = 0;
        private const double ClickCooldown = 150; // milliseconds
        private SoundEffect _buttonClickSound;


        private class Button
        {
            public Rectangle Bounds { get; set; }
            public string Text { get; set; }
            public Color Color { get; set; }
            private SoundEffect _clickSound; // Add this field

            public Button(Rectangle bounds, string text, Color color, SoundEffect clickSound) // Modify constructor
            {
                Bounds = bounds;
                Text = text;
                Color = color;
                _clickSound = clickSound; // Initialize the sound effect
            }

            public bool IsClicked(MouseState mouseState)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && Bounds.Contains(mouseState.Position))
                {
                    _clickSound?.Play(); // Play the sound effect on click
                    return true;
                }
                return false;
            }

            // No need to change the Draw method
            public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel)
            {
                spriteBatch.Draw(pixel, Bounds, Color);
                Vector2 textSize = font.MeasureString(Text);
                Vector2 textPosition = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, Text, textPosition, Color.Black);
            }
        }


        private SpriteFont _font;
        private Texture2D _pixel;
        private List<Button> _mainButtons;
        private List<Button> _levelButtons;
        private List<Button> _victoryButtons; // Add a list for victory buttons
        private MenuState _currentState;
        private string _victoryMessage; // Field to store victory message

        public event EventHandler<string> PlayRequested;
        public event EventHandler ExitRequested;

        public MainMenu(GraphicsDevice graphicsDevice, SpriteFont font, SoundEffect buttonClickSound)
        {
            _font = font;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _buttonClickSound = buttonClickSound; // Assign the sound effect

            InitializeButtons(graphicsDevice.Viewport);
            _currentState = MenuState.Main;
        }


        private void InitializeButtons(Viewport viewport)
        {
            int buttonWidth = 200;
            int buttonHeight = 50;
            int centerX = viewport.Width / 2 - buttonWidth / 2;
            int startY = viewport.Height / 2 - 100;

            _mainButtons = new List<Button>
    {
        new Button(new Rectangle(centerX, startY, buttonWidth, buttonHeight), "Play", Color.LightGray, _buttonClickSound),
        new Button(new Rectangle(centerX, startY + 70, buttonWidth, buttonHeight), "Select Level", Color.LightGray, _buttonClickSound),
        new Button(new Rectangle(centerX, startY + 140, buttonWidth, buttonHeight), "Exit", Color.LightGray, _buttonClickSound)
    };

            _levelButtons = new List<Button>
    {
        new Button(new Rectangle(centerX, startY, buttonWidth, buttonHeight), "Level 1", Color.LightGray, _buttonClickSound),
        new Button(new Rectangle(centerX, startY + 70, buttonWidth, buttonHeight), "Level 2", Color.LightGray, _buttonClickSound),
        new Button(new Rectangle(centerX, startY + 140, buttonWidth, buttonHeight), "Back", Color.LightGray, _buttonClickSound)
    };

            _victoryButtons = new List<Button>
    {
        new Button(new Rectangle(centerX, startY + 70, buttonWidth, buttonHeight), "Restart", Color.LightGray, _buttonClickSound),
        new Button(new Rectangle(centerX, startY + 140, buttonWidth, buttonHeight), "Back to Main Menu", Color.LightGray, _buttonClickSound)
    };
        }


        public void Update(MouseState mouseState, GameTime gameTime)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            if (currentTime - _lastClickTime < ClickCooldown)
            {
                return; // Skip updates if within cooldown period
            }
            _lastClickTime = currentTime;

            switch (_currentState)
            {
                case MenuState.Main:
                    UpdateMainMenu(mouseState);
                    break;
                case MenuState.LevelSelect:
                    UpdateLevelSelect(mouseState);
                    break;
                case MenuState.Victory:
                    UpdateVictoryState(mouseState);
                    break;
            }
        }

        private void UpdateMainMenu(MouseState mouseState)
        {
            if (_mainButtons[0].IsClicked(mouseState))
            {
                PlayRequested?.Invoke(this, "last");
            }
            else if (_mainButtons[1].IsClicked(mouseState))
            {
                _currentState = MenuState.LevelSelect;
            }
            else if (_mainButtons[2].IsClicked(mouseState))
            {
                ExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateLevelSelect(MouseState mouseState)
        {
            if (_levelButtons[0].IsClicked(mouseState))
            {
                PlayRequested?.Invoke(this, "lvl1");
            }
            else if (_levelButtons[1].IsClicked(mouseState))
            {
                PlayRequested?.Invoke(this, "lvl2");
            }
            else if (_levelButtons[2].IsClicked(mouseState))
            {
                _currentState = MenuState.Main;
            }
        }

        private void UpdateVictoryState(MouseState mouseState)
        {
            foreach (var button in _victoryButtons)
            {
                if (button.IsClicked(mouseState))
                {
                    if (button.Text == "Restart")
                    {
                        _currentState = MenuState.Main; // or use another method to restart the level
                        _victoryMessage = null; // Clear victory message
                        PlayRequested?.Invoke(this, "last"); // Restart the last level
                    }
                    else if (button.Text == "Back to Main Menu")
                    {
                        _currentState = MenuState.Main;
                        _victoryMessage = null; // Clear victory message
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            switch (_currentState)
            {
                case MenuState.Main:
                    foreach (var button in _mainButtons)
                    {
                        button.Draw(spriteBatch, _font, _pixel);
                    }
                    break;
                case MenuState.LevelSelect:
                    foreach (var button in _levelButtons)
                    {
                        button.Draw(spriteBatch, _font, _pixel);
                    }
                    break;
                case MenuState.Victory:
                    // Draw the victory message
                    if (!string.IsNullOrEmpty(_victoryMessage))
                    {
                        spriteBatch.DrawString(_font, _victoryMessage,
                            new Vector2(spriteBatch.GraphicsDevice.Viewport.Width / 2,
                                        spriteBatch.GraphicsDevice.Viewport.Height / 2 - 100),
                            Color.Gold,
                            0f,
                            _font.MeasureString(_victoryMessage) / 2,
                            1f,
                            SpriteEffects.None,
                            0f);
                    }

                    // Draw victory buttons
                    foreach (var button in _victoryButtons)
                    {
                        button.Draw(spriteBatch, _font, _pixel);
                    }
                    break;
            }

            spriteBatch.End();
        }

        public void ShowVictoryMessage(string message)
        {
            _victoryMessage = message;
            _currentState = MenuState.Victory;
        }
    }
}

