using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Task1
{
    public partial class Form1 : Form
    {
        // Переменная для хранения выбранного маркера
        private GMapMarker _selectedMarker;

        public Form1()
        {
            InitializeComponent();

            // Подписка на события мыши

            // Событие нажатия на кнопку мыши (как нажатие, так и зажатие)
            gMapControl1.MouseDown += _gMapControl_MouseDown;

            // Событие отжатия кнопки мыши
            gMapControl1.MouseUp += _gMapControl_MouseUp;
        }

        // Загрузка карты
        private void gMapControl1_Load(object sender, EventArgs e)
        {
            // Выбор подгрузки карты – онлайн или из ресурсов
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            // Какой провайдер карт используется (в нашем случае гугл)
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;

            // Минимальный зум
            gMapControl1.MinZoom = 2;

            // Максимальный зум
            gMapControl1.MaxZoom = 16;

            // Какой используется зум при открытии
            gMapControl1.Zoom = 4;

            // Точка в центре карты при открытии (центр России)
            gMapControl1.Position = new GMap.NET.PointLatLng(66.4169575018027, 94.25025752215694);

            // Как приближает (просто в центр карты или по положению мыши)
            gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;

            // Перетаскивание карты мышью
            gMapControl1.CanDragMap = true;

            // Какой кнопкой осуществляется перетаскивание
            gMapControl1.DragButton = MouseButtons.Left;

            // Показывать или скрывать красный крестик в центре
            gMapControl1.ShowCenter = false;

            // Показывать или скрывать тайлы
            gMapControl1.ShowTileGridLines = false;
        }

        // Создание маркера на карте
        private GMarkerGoogle GetMarker(string name, double x, double y, GMarkerGoogleType gMarkerGoogleType = GMarkerGoogleType.red)
        {
            // Широта, долгота, тип маркера
            GMarkerGoogle mapMarker = new GMarkerGoogle(new GMap.NET.PointLatLng(x, y), gMarkerGoogleType);

            // Всплывающее окно с инфой к маркеру
            mapMarker.ToolTip = new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(mapMarker);

            // Текст внутри всплывающего окна
            mapMarker.ToolTipText = name;

            // Отображение всплывающего окна (при наведении)
            mapMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

            return mapMarker;
        }

        // Создание слоя и добавление маркера на него
        private GMapOverlay GetOverlayMarkers(string name, double x, double y, GMarkerGoogleType gMarkerGoogleType = GMarkerGoogleType.red)
        {
            // Создание именованного слоя
            GMapOverlay gMapMarkers = new GMapOverlay(name);

            // Добавление маркеров на слой
            gMapMarkers.Markers.Add(GetMarker(name, x, y, gMarkerGoogleType));

            return gMapMarkers;
        }

        // Событие загрузки формы
        private void Form1_Load(object sender, EventArgs e)
        {
            // Данные для подключения к БД
            string connData = @"Data Source=.\SQLEXPRESS;Initial Catalog=gmap_test;User ID=dbo;Integrated Security=false;Integrated Security=SSPI";

            // Создаем объект класса SqlConnection передавая в конструктор данные для подключения к БД
            using (SqlConnection conn = new SqlConnection(connData))
            {
                // Объявляем локальную переменную для хранения имени маркера
                string markerName = "";

                // Объявляем локальную переменную для хранения широты маркера
                double firstCoordinate = 0.0;

                // Объявляем локальную переменную для хранения долготы маркера
                double secondCoordinate = 0.0;

                // Открываем соединение с БД
                conn.Open();

                // Создаем объект SqlCommand для выполнения SQL запроса
                SqlCommand command = new SqlCommand("SELECT * FROM Markers", conn);

                // Создаем объект SqlDataReader для получения результатов запроса и запускаем запрос
                SqlDataReader reader = command.ExecuteReader();

                // Проверяем получена ли хоть какая-нибудь информация
                if (reader.HasRows) 
                {
                    // Если получена, то считываем ее, пока она есть
                    while (reader.Read())
                    {
                        // Получаем имя маркера
                        markerName = reader["marker_name"].ToString();

                        // Получаем широту маркера
                        firstCoordinate = double.Parse(reader["first_coordinate"].ToString());

                        // Получаем долготу маркера
                        secondCoordinate = double.Parse(reader["second_coordinate"].ToString());

                        // Создаем маркер на карте
                        gMapControl1.Overlays.Add(GetOverlayMarkers(markerName, firstCoordinate, secondCoordinate));
                    }

                    // Обновляем маркеры
                    gMapControl1.Update();
                }

                // Отключаемся от БД
                conn.Close();
                conn.Dispose();
            }
        }

        // Событие нажатия/зажатия кнопки мыши
        private void _gMapControl_MouseDown(object sender, MouseEventArgs e)
        {
            //находим тот маркер над которым нажали клавишу мыши
            _selectedMarker = gMapControl1.Overlays
                .SelectMany(o => o.Markers)
                .FirstOrDefault(m => m.IsMouseOver == true);
        }

        // Событие отжатия кнопки мыши
        private void _gMapControl_MouseUp(object sender, MouseEventArgs e)
        {
            // Если маркер не выбран, то выходим из функции
            if (_selectedMarker is null)
                return;

            //переводим координаты курсора мыши в долготу и широту на карте
            var latlng = gMapControl1.FromLocalToLatLng(e.X, e.Y);

            //присваиваем новую позицию для маркера
            _selectedMarker.Position = latlng;

            // Переменная для хранения имени перемещаемого маркера
            string markerName = _selectedMarker.ToolTipText;

            // Переменная для хранения широты маркера
            string firstCoordinate = latlng.Lat.ToString();

            // Переменная для хранения долготы маркера
            string secondCoordinate = latlng.Lng.ToString();

            // Данные для подключения к БД
            string connData = @"Data Source=.\SQLEXPRESS;Initial Catalog=gmap_test;User ID=dbo;Integrated Security=false;Integrated Security=SSPI";

            // Создаем объект класса SqlConnection передавая в конструктор данные для подключения к БД
            using (SqlConnection conn = new SqlConnection(connData))
            {
                // Открываем соединение с БД
                conn.Open();

                // Создаем объект SqlCommand для выполнения SQL запроса
                SqlCommand command = new SqlCommand($"UPDATE Markers SET first_coordinate='{firstCoordinate}', second_coordinate='{secondCoordinate}' WHERE marker_name='{markerName}'", conn);

                // Создаем объект SqlDataReader для получения результатов запроса и запускаем запрос
                SqlDataReader reader = command.ExecuteReader();

                // Отключаемся от БД
                conn.Close();
                conn.Dispose();
            }

            // Удаляем выбранный маркер из переменной (обнуляем переменную)
            _selectedMarker = null;
        }
    }
}
