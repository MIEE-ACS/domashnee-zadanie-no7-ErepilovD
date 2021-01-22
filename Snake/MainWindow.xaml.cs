﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        //количество очков
        int score;
        //таймер
        DispatcherTimer moveTimer;

        //дополнительная жизнь (её наличие сейчас, была ли использована до, 
        //наличие на экране сейчас, врезалась ли во что-то змея)
        bool IsExtraLife, UseOfExtraLife, ELifeOnlyOnScreen, SmashOrEat;
        ExtraLife heart;
        //рандомное целое число до 10
        int rnd;

        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();

            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);

            //рандомизируем после какого количества яблок появится сердце
            Random rand = new Random();
            rnd = rand.Next(11);

        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    if (IsExtraLife)
                    {
                        //если есть доп жизнь
                        IsExtraLife = false;
                        UseOfExtraLife = true;
                        label2.Visibility = Visibility.Hidden;
                        SmashOrEat = true;
                        moveTimer.Stop();
                    }
                    else
                    {
                        //мы проиграли
                        moveTimer.Stop();
                        tbGameOver.Visibility = Visibility.Visible;
                        button1.Visibility = Visibility.Visible;
                        scoreCanvas.Visibility = Visibility.Hidden;
                        return;
                    }
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                if (IsExtraLife)
                {
                    //если есть доп жизнь
                    IsExtraLife = false;
                    UseOfExtraLife = true;
                    label2.Visibility = Visibility.Hidden;
                    SmashOrEat = true;
                    moveTimer.Stop();
                }
                else
                {
                    //мы проиграли
                    moveTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    button1.Visibility = Visibility.Visible;
                    scoreCanvas.Visibility = Visibility.Hidden;
                    return;
                }
            }

            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                score++;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);
            }

            //если жизнь появилась на экране, но не была съедена
            if (ELifeOnlyOnScreen)
            {
                Canvas.SetTop(heart.image, heart.y);
                Canvas.SetLeft(heart.image, heart.x);
                if (head.x == heart.x && head.y == heart.y)
                {
                    IsExtraLife = true;
                    ELifeOnlyOnScreen = false;
                    canvas1.Children.Remove(heart.image);
                    label2.Visibility = Visibility.Visible;
                }
            }

            //иначе если если у нас нет доп жизни и мы её не использовали, 
            //и счётчик дошёл до какого-то числа
            else if ((score == rnd) && (!UseOfExtraLife) && (!IsExtraLife))
            {
                canvas1.Children.Add(heart.image);
                heart.move();
                Canvas.SetTop(heart.image, heart.y);
                Canvas.SetLeft(heart.image, heart.x);
                ELifeOnlyOnScreen = true;
            }

            //перерисовываем экран
            if (!SmashOrEat) { UpdateField(); }
            //смещаем змею на одну клетку назад
            else
            {
                foreach (var p in snake)
                {
                    p.ifDeadWithElife();
                }

            }
        }

        // Обработчик нажатия на кнопку клавиатуры
        //если змея умерла с доп жизнью, то нажатие запускает таймер
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (SmashOrEat) { SmashOrEat = false; moveTimer.Start(); }
                    head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                    if (SmashOrEat) { SmashOrEat = false; moveTimer.Start(); }
                    head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                    if (SmashOrEat) { SmashOrEat = false; moveTimer.Start(); }
                    head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                    if (SmashOrEat) { SmashOrEat = false; moveTimer.Start(); }
                    head.direction = Head.Direction.RIGHT;
                    break;
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;

            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);

            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);

            //запускаем таймер
            moveTimer.Start();
            UpdateField();
            //скрываем кнопку Start
            button1.Visibility = Visibility.Hidden;
            //показываем счёт
            scoreCanvas.Visibility = Visibility.Visible;

            //доп. жизнь
            heart = new ExtraLife(snake);
            IsExtraLife = false;
            UseOfExtraLife = false;
            ELifeOnlyOnScreen = false;
            SmashOrEat = false;
        }

        public class Entity
        {
            protected int m_width;
            protected int m_height;

            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }
            public virtual void ifDeadWithElife() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        //класс с доп жизнью
        public class ExtraLife : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public ExtraLife(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "C:/Users/Dmitriy/source/repos/domashnee-zadanie-no7-ErepilovD-master/Snake/Resources/heart.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(6) * 40 + 40;
                    y = rand.Next(6) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction
            {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }

            //смещаем на одну клетку назад
            public override void ifDeadWithElife()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y -= 40;
                        break;
                    case Direction.UP:
                        y += 40;
                        break;
                    case Direction.LEFT:
                        x += 40;
                        break;
                    case Direction.RIGHT:
                        x -= 40;
                        break;
                }
            }
        }
        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
            public override void ifDeadWithElife()
            {
                x = m_next.x;
                y = m_next.y;
            }

        }

    }
}
