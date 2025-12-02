using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    class Screen
    {
        private Texture _bgTexture;
        private Texture _titleTexture;
        private Texture _startButtonTexture;
        private UI _userInterface;
        private Vector2 _buttonPos;
        private Vector2 _buttonSize;
        private Vector2 _titlePos;
        private Vector2 _titleSize;
        private bool _buttonHover = false;
        private bool _prevLeftDown = false;
        private readonly Vector2 TitleRatio = new Vector2(0.8f, 0.2f);
        private readonly Vector2 ButtonRatio = new Vector2(0.2f, 0.1f);

        public Screen(Vector2 Size, UI ui, Texture bgTexture, Texture titleTexture, Texture buttonTexture)
        {
            _bgTexture = bgTexture;
            _titleTexture = titleTexture;
            _startButtonTexture = buttonTexture;

            _titleSize = new Vector2(Size.X * TitleRatio.X, Size.Y * TitleRatio.Y);
            _buttonSize = new Vector2(Size.X * ButtonRatio.X, Size.Y * ButtonRatio.Y);
            _titlePos = new Vector2((Size.X - _titleSize.X) / 2f, Size.Y * 0.12f);
            _buttonPos = new Vector2((Size.X - _buttonSize.X) / 2f, Size.Y * 0.6f);

            _userInterface = ui;
        }
        public void RenderFrame(Shader shader, Vector2 Size)
        {
            shader.Use();
            var proj = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1f, 1f);
            shader.SetMatrix4("projection", proj);

            if (_bgTexture != null)
            {
                float tileSize = 256f;
                Vector2 tiling = new Vector2(Size.X / tileSize, Size.Y / tileSize);
                _userInterface.DrawTexture(_bgTexture, new Vector2(0f, 0f), Size, shader, 1f, tiling);
            }

            if (_titleTexture != null)
            {
                _userInterface.DrawTexture(_titleTexture, _titlePos, _titleSize, shader, 1f);
            }

            if (_startButtonTexture != null)
            {
                float scale = _buttonHover ? 1.06f : 1.0f;
                _userInterface.DrawTexture(_startButtonTexture, _buttonPos, _buttonSize, shader, scale);
            }
        }

        public void Resize(Vector2 Size)
        {
            _titleSize = new Vector2(Size.X * TitleRatio.X, Size.Y * TitleRatio.Y);
            _buttonSize = new Vector2(Size.X * ButtonRatio.X, Size.Y * ButtonRatio.Y);
            _titlePos = new Vector2((Size.X - _titleSize.X) / 2f, Size.Y * 0.12f);
            _buttonPos = new Vector2((Size.X - _buttonSize.X) / 2f, Size.Y * 0.6f);
        }

        public bool CheckButtonClick(Vector2 mousePos, bool leftDown)
        {
            float mx = mousePos.X;
            float my = mousePos.Y;
            _buttonHover = mx >= _buttonPos.X && mx <= _buttonPos.X + _buttonSize.X
                    && my >= _buttonPos.Y && my <= _buttonPos.Y + _buttonSize.Y;

            if (leftDown && !_prevLeftDown && _buttonHover)
            {
                return true;
            }

            _prevLeftDown = leftDown;
            return false;
        }

        public void Dispose()
        {
            if (_bgTexture != null) GL.DeleteTexture(_bgTexture.Handle);
            if (_titleTexture != null) GL.DeleteTexture(_titleTexture.Handle);
            if (_startButtonTexture != null) GL.DeleteTexture(_startButtonTexture.Handle);
        }
    }
}