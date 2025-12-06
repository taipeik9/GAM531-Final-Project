using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    class Screen
    {
        private readonly Texture _bgTexture;
        private readonly Texture _titleTexture;
        private readonly Texture _mainButtonTexture;
        private readonly Texture _secondaryButtonTexture;
        private readonly UI _userInterface;
        private Vector2 _mainButtonPos;
        private Vector2 _secondaryButtonPos;
        private Vector2 _buttonSize;
        private Vector2 _titlePos;
        private Vector2 _titleSize;
        private bool _mainButtonHover = false;
        private bool _secondaryButtonHover = false;
        private bool _prevLeftDown = false;
        private bool _prevSecondaryLeftDown = false;
        private readonly Vector2 TitleRatio = new Vector2(0.8f, 0.2f);
        private readonly Vector2 ButtonRatio = new Vector2(0.2f, 0.1f);

        public Screen(Vector2 Size, UI ui, Texture bgTexture, Texture titleTexture, Texture buttonTexture, Texture? secondaryButtonTexture = null)
        {
            _bgTexture = bgTexture;
            _titleTexture = titleTexture;
            _mainButtonTexture = buttonTexture;

            _titleSize = new Vector2(Size.X * TitleRatio.X, Size.Y * TitleRatio.Y);
            _buttonSize = new Vector2(Size.X * ButtonRatio.X, Size.Y * ButtonRatio.Y);
            _titlePos = new Vector2((Size.X - _titleSize.X) / 2f, Size.Y * 0.12f);
            _mainButtonPos = new Vector2((Size.X - _buttonSize.X) / 2f, Size.Y * 0.6f);

            if (secondaryButtonTexture != null)
            {
                _secondaryButtonTexture = secondaryButtonTexture;
                _secondaryButtonPos = new Vector2((Size.X - _buttonSize.X) / 2f, (Size.Y * 0.6f) + _buttonSize.Y);
            }

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

            if (_mainButtonTexture != null)
            {
                float scale = _mainButtonHover ? 1.06f : 1.0f;
                _userInterface.DrawTexture(_mainButtonTexture, _mainButtonPos, _buttonSize, shader, scale);
            }
            if (_secondaryButtonTexture != null)
            {
                float scale = _secondaryButtonHover ? 1.06f : 1.0f;
                _userInterface.DrawTexture(_secondaryButtonTexture, _secondaryButtonPos, _buttonSize, shader, scale);
            }
        }

        public void Resize(Vector2 Size)
        {
            _titleSize = new Vector2(Size.X * TitleRatio.X, Size.Y * TitleRatio.Y);
            _buttonSize = new Vector2(Size.X * ButtonRatio.X, Size.Y * ButtonRatio.Y);
            _titlePos = new Vector2((Size.X - _titleSize.X) / 2f, Size.Y * 0.12f);
            _mainButtonPos = new Vector2((Size.X - _buttonSize.X) / 2f, Size.Y * 0.6f);
            _secondaryButtonPos = new Vector2((Size.X - _buttonSize.X) / 2f, (Size.Y * 0.6f) + _buttonSize.Y);
        }

        public bool CheckMainButtonClick(Vector2 mousePos, bool leftDown)
        {
            float mx = mousePos.X;
            float my = mousePos.Y;
            _mainButtonHover = mx >= _mainButtonPos.X && mx <= _mainButtonPos.X + _buttonSize.X
                    && my >= _mainButtonPos.Y && my <= _mainButtonPos.Y + _buttonSize.Y;

            if (leftDown && !_prevLeftDown && _mainButtonHover)
            {
                return true;
            }

            _prevLeftDown = leftDown;
            return false;
        }
        public bool CheckSecondaryButtonClick(Vector2 mousePos, bool leftDown)
        {
            if (_secondaryButtonTexture == null)
            {
                return false;
            }
            float mx = mousePos.X;
            float my = mousePos.Y;
            _secondaryButtonHover = mx >= _secondaryButtonPos.X && mx <= _secondaryButtonPos.X + _buttonSize.X
                    && my >= _secondaryButtonPos.Y && my <= _secondaryButtonPos.Y + _buttonSize.Y;

            if (leftDown && !_prevSecondaryLeftDown && _secondaryButtonHover)
            {
                return true;
            }

            _prevSecondaryLeftDown = leftDown;
            return false;
        }

        public void Dispose()
        {
            if (_bgTexture != null) GL.DeleteTexture(_bgTexture.Handle);
            if (_titleTexture != null) GL.DeleteTexture(_titleTexture.Handle);
            if (_mainButtonTexture != null) GL.DeleteTexture(_mainButtonTexture.Handle);
        }
    }
}