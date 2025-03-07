using System.Collections;
using SadConsole.Entities;
using SadConsole.Input;

namespace SpaceShooter;

using SadConsole;
using SadRogue.Primitives;

public class GameScreen : ScreenObject
    {
        private Console _console;
        private Player _player;
        private List<Bullet> _bullets;
        private List<Enemy> _enemies;
        private List<IMoveable> _moveableEntities;
        private Random _random;
        private int _score;
        private int _health = 3;

        public GameScreen()
        {
            _console = new Console(40, 25);
            Children.Add(_console);

            _player = new Player();
            _player.Position = new Point(20, 22);
            _console.Children.Add(_player);

            _bullets = new List<Bullet>();
            _enemies = new List<Enemy>();
            _random = new Random();
            
            Game.Instance.FrameUpdate += OnFrameUpdate;
        }
        
        public override bool ProcessKeyboard(Keyboard info)
        {
            if (info.IsKeyPressed(Keys.Left) && _player.Position.X > 0)
                _player.Position += new Point(-1, 0);
            else if (info.IsKeyPressed(Keys.Right) && _player.Position.X < 39)
                _player.Position += new Point(1, 0);
            
            if (info.IsKeyPressed(Keys.Up) && _player.Position.Y > 0)
                _player.Position += new Point(0, -1);
            else if (info.IsKeyPressed(Keys.Down) && _player.Position.Y < 25)
                _player.Position += new Point(0, 1);

            if (info.IsKeyPressed(Keys.Space))
            {
                var bullet = new Bullet { Position = _player.Position + new Point(0, -1) };

                _bullets.Add(bullet);
                _console.Children.Add(bullet);
            }

            return base.ProcessKeyboard(info);
        }

        private void OnFrameUpdate(object? sender, GameHost e)
        {
            _console.Clear();
            _console.Print(1, 0, $"Score: {_score}");
            _console.Print(25, 0, $"Health: {_health}");
            
            _moveableEntities = _bullets.Cast<IMoveable>().Concat(_enemies).ToList();
            _moveableEntities.ForEach(x => x.Move());
            
            SpawnRandomEntities();
            
            UpdateProjectiles();
            UpdateEnemies();

            CheckPlayerEnemyCollisions();
            CheckBulletEnemyCollisions();
        }
        
        private void CheckPlayerEnemyCollisions()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                if (!_player.CheckCollision(enemy)) continue;
        
                RemoveEntity(enemy, _enemies);
                
                _health--;
                
                break;
            }
        }
        
        private void CheckBulletEnemyCollisions()
        {
            foreach (var bullet in _bullets.ToArray())
            foreach (var enemy in _enemies.ToArray())
            {
                if (bullet.Position != enemy.Position) continue;
        
                RemoveEntity(bullet, _bullets);
                RemoveEntity(enemy, _enemies);
                _score += 10;
                break;
            }
        }

        private void UpdateProjectiles()
        {
            foreach (var bullet in _bullets.ToArray())
            {
                if (bullet.Position.Y < 0)
                    RemoveEntity(bullet, _bullets);
            }
        }
        
        private void UpdateEnemies()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                if (enemy.Position.Y > 24)
                    RemoveEntity(enemy, _enemies);
            }
        }


        private void SpawnRandomEntities()
        {
            TrySpawnEntity(50, () =>
            {
                var enemy = new Enemy(){ Position = new Point(_random.Next(0, 40), 0) };

                return (enemy, _enemies);
            });
        }
        
        private void TrySpawnEntity(int chancePercent, Func<(ScreenSurface entity, IList collection)> createEntity)
        {
            if (_random.Next(0, 1000) >= chancePercent) return;
    
            var (entity, collection) = createEntity();
            collection.Add(entity);
            _console.Children.Add(entity);
        }

        private void RemoveEntity(ScreenSurface entity, IList collection)
        {
            collection.Remove(entity);
            _console.Children.Remove(entity);
        }
    }