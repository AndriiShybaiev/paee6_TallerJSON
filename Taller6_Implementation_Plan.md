# План реализации Taller 6: Acceso a datos JSON con WPF

## 0. Назначение и правила для агента

Реализуй только базовую практическую работу по `Taller6JSON.pdf`, используя существующую заготовку проекта. Это не задача на проектирование новой архитектуры. Результат должен воспроизводить структуру и поведение учебного примера.

Документ составлен по PDF и отдельно прочитанным изображениям страниц 2, 5–6, 10–25, 28–30. Страница 9 содержит ссылку на предоставленный файл провинций; не нужно заново собирать набор данных. Настройки со скриншотов страниц 3–4, 7–8, 26–27 подробно не расшифрованы. Точный target framework и версии пакетов не установлены этим планом — проверь заготовку.

### 0.1. Приоритет источников

1. Явные указания пользователя и ограничения существующей заготовки.
2. Фактически видимые требования и код PDF.
3. Этот план как расшифровка PDF и порядок реализации.
4. Минимальные технические адаптации для сборки существующего решения.

Если заготовка отличается от примера, не удаляй и не переписывай её автоматически: сначала установи, какие части уже работают. Если обнаружено существенное противоречие, опиши его, не придумывай требование.

Код ниже — восстановленный учебный образец. Его можно использовать как основу, но он не был скомпилирован в окружении пользователя. Особенно проверь совместимость API пакета PDF. Не объявляй проект готовым только потому, что вставлен весь код.

### 0.2. Рабочий режим

- Делай этапы последовательно: 0 → 1 → 2 → 3 → 4 → 5 → 6.
- За один запуск выполняй один указанный этап. Закончи его проверкой и коротким отчётом. Не начинай следующий без команды пользователя.
- Перед изменениями прочитай реальные файлы, а не полагайся на предполагаемую структуру.
- Сохраняй существующий target framework, имена проектов и рабочие настройки, если нет необходимости их менять.
- Не обновляй все пакеты и .NET до последних версий ради этой работы.
- Не добавляй MVVM, DI, ORM, базу данных, веб-интерфейс, асинхронный фреймворк, универсальные сервисы и новые проекты без необходимости.
- Не добавляй функции страницы 31: поиск, пагинацию, изображения, дополнительные поля, XML/CSV, редизайн, расширенное оформление PDF.
- Не выполняй массовую очистку и форматирование посторонних файлов.
- Не делай коммиты, не публикуй проект и не удаляй пользовательские изменения без отдельного разрешения.
- Не подменяй отсутствующие данные вымышленным списком провинций.
- Если Windows/WPF runtime недоступен, отделяй статическую проверку, результат сборки и реально выполненные GUI-тесты. Не утверждай, что интерфейс проверен запуском.

### 0.3. Состояние между этапами

После этапа обновляй один небольшой файл `IMPLEMENTATION_STATUS.md` в корне решения:

```markdown
# Состояние реализации
- Завершённый этап:
- Изменённые файлы:
- Команда сборки и её результат:
- Выполненные проверки:
- Не выполнено и почему:
- Отклонения от слайдов и причина:
- Следующий этап:
```

Это рабочий журнал для агента, не обязательный отчёт преподавателю. Не копируй в него весь план или исходный код.

## 1. Ожидаемый результат

Windows-приложение WPF «Gestión de Provincias», которое:

1. Загружает предоставленный JSON через диалог выбора файла.
2. Показывает вертикальный прокручиваемый список провинций.
3. Для каждой провинции показывает редактируемые `ID` и `Nombre`.
4. Переносит изменения полей в объект модели при потере фокуса.
5. Сортирует текущий список по ID или названию.
6. Сохраняет текущий список в JSON: в текущий файл или через «Сохранить как».
7. При закрытии с загруженными данными предлагает сохранить, выйти без сохранения либо отменить закрытие.
8. Экспортирует текущий список в PDF с заголовком и таблицей.
9. После успешного экспорта предлагает открыть PDF.

Не требуются добавление/удаление провинций: соответствующие операции не показаны в разобранном базовом коде. Не добавляй их по собственной инициативе.

## 2. Структура решения и зависимости

Ожидаемая схема:

```text
Solution
├── UserInterface                    WPF-приложение, запускаемый проект
│   ├── App.xaml / App.xaml.cs        существующие файлы приложения
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ProvinciaUserControl.xaml
│   └── ProvinciaUserControl.xaml.cs
├── DataAccess                       библиотека классов
│   ├── Provincia.cs
│   ├── ProvinciasJSON.cs
│   └── Newtonsoft.Json              NuGet
├── ExportPDF                        библиотека классов
│   ├── ProvinciasPDF.cs
│   └── PDFsharp-MigraDoc-WPF         пакет, названный в презентации
└── предоставленный provincias.json расположение определяется заготовкой
```

Связи:

```text
UserInterface → DataAccess
UserInterface → ExportPDF
ExportPDF → DataAccess
DataAccess → Newtonsoft.Json
ExportPDF → библиотеки MigraDoc / PDFsharp
```

Не создавай циклические ссылки. Пространства имён примера: `UserInterface`, `DataAccess`, `ExportPDF`.

Если заготовка уже содержит подходящие проекты, используй их. Если имена отличаются, согласованно адаптируй пространства имён и `x:Class`; не создавай параллельный второй интерфейс только ради названия.

Исходный JSON выбирается через `OpenFileDialog`: жёсткий абсолютный путь преподавателя из скриншота не нужен. Не требуется автоматическая загрузка при запуске. Не требуется встраивать список провинций в C#.

## 3. Детальная спецификация поведения

### 3.1. Главное окно — страницы 2, 5–6

- `Title="Gestión de Provincias"`, `Height="450"`, `Width="800"`.
- Основной контейнер `Grid`.
- Верхняя панель `botonesStackPanel`: горизонтальная, высота 40.
- В центре `ScrollViewer` с отступом `0,40,0,25`, внутри вертикальный `provinciasStackPanel`.
- Внизу `StatusBar`, высота 25, текстовый элемент `statusText`.
- При запуске список пуст, статус `Fichero no cargado`.

| Текст кнопки | Имя | Обработчик | При запуске |
|---|---|---|---|
| Cargar | cargarButton | cargarButton_Click | включена |
| Ordenar por ID | idButton | idButton_Click | отключена |
| Ordenar por Nombre | nombreButton | nombreButton_Click | отключена |
| Guardar | guardarButton | guardarButton_Click | отключена |
| Guardar Como | guardarComoButton | guardarComoButton_Click | отключена |
| Exportar a PDF | exportarButton | exportarButton_Click | отключена |

Сохрани испанские подписи. Не заменяй интерфейс на DataGrid или новое меню: в примере используются отдельные UserControl внутри StackPanel.

### 3.2. Модель и доступ к JSON — страницы 10–13

`Provincia`: публичные изменяемые свойства `int ID`, `string Nombre`.

`ProvinciasJSON`:

- изменяемое публичное свойство `Path`;
- конструктор принимает путь, но не загружает файл;
- `getProvincias()` читает весь файл и десериализует `List<Provincia>` через Newtonsoft.Json;
- при перехваченном `JsonReaderException` возвращает `null`;
- `saveProvincias(...)` сериализует переданный список и записывает по `Path`;
- возвращает `true` при успехе, `false` при перехваченном исключении;
- список не хранится внутри `ProvinciasJSON`.

### 3.3. Компонент провинции — страницы 14–18

- `ProvinciaUserControl` находится в проекте интерфейса.
- Высота 30; `d:DesignWidth="350"` — это ширина дизайнера, не обязательная runtime-ширина.
- Два поля `idTextBox` и `nombreTextBox`, подписи ID / Nombre.
- Свойство `Provincia` сохраняет ссылку на модель и заполняет оба поля.
- Getter возвращает ту же модель.
- `idTextBox_LostFocus`: `Convert.ToInt32`; при `FormatException` модель и поле получают 0.
- `nombreTextBox_LostFocus`: текст записывается в `provincia.Nombre`.
- Компонент не сохраняет JSON самостоятельно.
- В примере нет Binding, DataContext, INotifyPropertyChanged: обновление ручное.

### 3.4. Загрузка и отображение — страницы 19–21

Главное окно хранит `provinciasJSON` и `provincias`.

1. Открыть `OpenFileDialog`, фильтр `Archivo JSON|*.json`.
2. При отмене ничего не менять.
3. Создать `ProvinciasJSON` для выбранного файла.
4. Получить список.
5. Если список не null: показать компоненты, включить пять кнопок, вывести путь в статус.
6. Если null: очистить панель, отключить пять кнопок, показать `Error cargando datos` с заголовком `Error`, такой же текст вывести в статус.

`showProvincias()` очищает панель и создаёт по одному `ProvinciaUserControl` на объект. Свойство `Provincia` компонента получает сам объект из списка, а не его копию.

`activarBotones(bool)` меняет доступность всех кнопок верхней панели, кроме `cargarButton`.

Пустой список `[]` отличается от null: считается успешно загруженным, кнопки включаются.

### 3.5. Сортировка — страница 25

- `orderProvincias(false)` → `OrderBy(o => o.ID).ToList()`.
- `orderProvincias(true)` → `OrderBy(o => o.Nombre).ToList()`.
- После каждой сортировки вызвать `showProvincias()`.
- Сортировка только прямая; повторное нажатие не меняет направление.
- Не сортировать автоматически при загрузке, сохранении или экспорте.

### 3.6. Сохранение — страницы 22–23

`guardar()` вызывает `saveProvincias(provincias)`:

- ошибка: `Error guardando datos`, заголовок `Error`, вернуть false;
- успех: `Datos guardados correctamente en ` + текущий путь, заголовок `Datos guardados`, вернуть true.

`Guardar` просто вызывает `guardar()`.

`Guardar Como`:

1. `SaveFileDialog`, фильтр `Archivo JSON|*.json`.
2. Отмена ничего не меняет.
3. После выбора присвоить `provinciasJSON.Path = выбранный путь`.
4. Сохранить список.
5. При успехе обновить путь в статусе и показать сообщение.
6. При ошибке показать `Error guardando datos en ` + путь.

После успешного Save As обычное сохранение использует новый путь.

### 3.7. Закрытие — страница 24

- При `provincias == null` закрытие без вопроса.
- Иначе показать `¿Quiere guardar los datos antes de salir?`.
- Заголовок `Saliendo de la aplicación`, кнопки YesNoCancel, значок Question.
- Yes: вызвать `guardar()`; закрыть только при true.
- No: закрыть без сохранения.
- Cancel: отменить закрытие.
- Учёт «грязного» состояния не нужен: вопрос появляется при любом ненулевом списке, даже после сохранения.

Обязательно подключить обработчик события Closing ровно один раз. В XAML страницы 6 подписки нет; тело конструктора на странице 19 свёрнуто. Способ подключения — техническое дополнение, не дословно прочитанный код.

### 3.8. PDF — страницы 28–30

`ProvinciasPDF.SavePDF(List<Provincia>, string path)`:

1. Создать Document и Section.
2. Заголовок `Provincias`: размер 20, bold, по центру.
3. Пустой абзац.
4. Таблица: ID шириной 2cm, Provincia шириной 10cm.
5. Первый столбец по центру, второй слева.
6. Строка заголовков: `ID`, `Provincia`, bold, LightBlue, HeadingFormat=true.
7. Для каждой провинции добавить ID и Nombre в порядке списка.
8. RenderDocument и сохранение в переданный путь.
9. true при успехе, false при перехваченном исключении.

Обработчик экспорта:

- SaveFileDialog, `Archivo PDF|*.pdf`;
- передать текущий список в SavePDF — не перечитывать JSON;
- при успехе сообщение `Datos exportados a PDF en <путь>.\n\n ¿Desea abrirlo?`, заголовок `Exportación correcta`, YesNo;
- при Yes открыть через Process / ProcessStartInfo с UseShellExecute=true;
- при No только оставить файл сохранённым;
- путь JSON и statusText из-за экспорта не менять.

В базовом PDF не требуются рамки, логотипы, даты, номера страниц, дополнительные поля или диаграммы.

## 4. Неопределённости и разрешённые минимальные адаптации

Не превращай этот раздел в дополнительные функции.

| Место | Что известно | Как действовать |
|---|---|---|
| Конструктор MainWindow | На слайде свёрнут | Вызвать InitializeComponent; подключить Closing один раз, если ещё не подключён |
| Nullable | getProvincias возвращает null при типе без ?; строка Nombre без начального значения | При включённом nullable разрешены корректные ? и начальные значения; не выключай проверки во всём решении |
| saveProvincias | На стр. 11 параметр с ?, на стр. 13 без ? | Используй вариант реализации стр. 13; передавай только загруженный список |
| PDF API | На слайдах есть FailsafeFontResolver из PdfSharp.Snippets.Font | Проверь доступность в установленном пакете; не выдумывай namespace или пакет |
| Стиль Table | Есть table.Style = "Table", но определения не показано | Проверь генерацию с реальной версией. Если ошибка из-за отсутствующего стиля — убери только это присваивание либо минимально определи стиль; зафиксируй отклонение |
| Выбор пакетов | В PDF назван PDFsharp-MigraDoc-WPF, версия не подтверждена | Сначала существующие PackageReference/lock-файлы, затем минимально совместимое решение |
| Фреймворк | Из расшифровки не установлен | Сохрани существующий совместимый Windows target framework; не угадывай версию из года PDF |

Если используемый WPF-вариант библиотеки не требует Core font resolver и snippet-класс отсутствует, допускается убрать только неактуальную настройку resolver вместе с ненужными using после проверки документации/пакета и реального экспорта. Не добавляй самодельный шрифтовой подсервис без необходимости.

### 4.1. Известные ограничения учебного кода

- ReadAllText расположен до try: ошибки чтения не перехватываются этим методом.
- Загрузка в MainWindow не содержит своего try/catch.
- ID ловит FormatException, но не OverflowException; уникальность и положительность не проверяются.
- Имя допускает пустую строку, пробелы не обрезаются.
- Повторное открытие файла не спрашивает о сохранении старых данных.
- Save As меняет Path до записи и не восстанавливает его при ошибке.
- SavePDF возвращает false, но обработчик экспорта не показывает ветку ошибки.
- Запуск открытия PDF не защищён отдельным try/catch.

Не воспроизводи аварийное поведение специально, но и не меняй автоматически семантику задания. В первую очередь воспроизведи нормальные сценарии. Допустимы узкие защитные проверки и сообщения об ошибках, если они нужны для стабильности; каждое такое изменение перечисли отдельно. Не добавляй dirty tracking, автосохранение, отмену редактирования и сложную валидацию.

## 5. Этапы выполнения

### Этап 0 — аудит заготовки, без реализации

Прочитай решение, csproj, App.xaml, окна, существующие классы, PackageReference, имеющийся JSON и инструкции репозитория.

Выведи:

- фактическое дерево проектов и ссылки;
- target frameworks и версии пакетов;
- какие классы/функции уже есть и чего не хватает;
- доступность dotnet/MSBuild и Windows GUI;
- исходный результат сборки, если сборка возможна;
- план минимальных изменений по следующим этапам.

Не удаляй и не пересоздавай проект. Зафиксируй исходные ошибки отдельно от будущих.

Готовность: структура понятна, заготовка сопоставлена с разделами 1–3, неопределённости записаны.

### Этап 1 — модель и JSON

Меняй только DataAccess и необходимые зависимости.

- Создай/дополни Provincia.
- Создай/дополни ProvinciasJSON.
- Подключи Newtonsoft.Json.
- Учти nullable точечно, если включён.
- Не меняй предоставленный исходный JSON ради проверки.

Проверь:

- DataAccess собирается;
- предоставленный JSON читается, ID/Nombre распознаются;
- тестовая копия списка сохраняется во временный файл и читается обратно;
- синтаксически некорректный JSON даёт ожидаемый null;
- `[]` даёт пустой ненулевой список.

Тестовые файлы не подменяют основной набор данных. Используй существующую тестовую инфраструктуру, если есть; не создавай новый тестовый фреймворк ради двух методов.

### Этап 2 — ProvinciaUserControl

Меняй компонент и только необходимые ссылки UserInterface → DataAccess.

- Создай User Control WPF в проекте интерфейса.
- Перенеси разметку и код из раздела 6.
- Сохрани имена и обработчики.
- Проверь присваивание модели и обновление по LostFocus.

Проверки: отображение начальных значений; корректный ID; буквы вместо ID → 0; редактирование имени → изменение того же объекта. Не требуй готовых загрузки и экспорта на этом этапе.

### Этап 3 — главное окно, загрузка и сортировка

- Вставь/адаптируй MainWindow.xaml.
- Реализуй поля, конструктор, cargarButton_Click, showProvincias, activarBotones, orderProvincias и две сортировки.
- Для сохранения/экспорта временно допустимы пустые обработчики только ради промежуточной сборки. Явно отметь их в журнале как незавершённые. Не оставляй их в финале.
- На этом этапе ещё не подключай отсутствующий Window_Closing.

Проверки: стартовое состояние; отмена открытия; успешная загрузка; обработанный null; отображение пути; повторная загрузка без дублирования строк; обе сортировки; изменения модели не теряются после перестроения списка.

### Этап 4 — сохранение и закрытие

- Замени временные обработчики сохранения реальной логикой.
- Реализуй guardar(), Guardar Como, Window_Closing.
- Подключи Closing ровно один раз.
- Не вводи признак unsaved changes.

Проверки: сохранение копии; Save As; последующее Save в новый путь; отмена Save As; закрытие без данных; Yes/No/Cancel; ошибка записи при Yes отменяет выход.

Все операции записи проверяй на копиях данных, не уничтожай исходный предоставленный JSON.

### Этап 5 — PDF

- Подготовь ExportPDF и ссылки.
- Проверь совместимость реального пакета с кодом resolver / renderer.
- Реализуй ProvinciasPDF и exportarButton_Click.
- Удали временную заглушку экспорта.

Проверки: экспорт списка; отмена диалога; заголовок и таблица; испанские символы; порядок строк после сортировки; актуальные изменённые значения; Yes/No при предложении открыть PDF; повторный экспорт в той же сессии; таблица с полным предоставленным списком.

Успех SavePDF недостаточен: открой PDF и проверь содержимое, если окружение позволяет. Если не позволяет, честно оставь визуальную проверку невыполненной.

### Этап 6 — интеграция и сдаваемое состояние

- Собери всё решение.
- Пройди матрицу раздела 7.
- Удали все временные обработчики, TODO и тестовый мусор, созданный в процессе.
- Не удаляй чужие TODO вне области задания.
- Проверь соответствие базовому примеру, отсутствие расширений страницы 31.
- Подготовь краткую инструкцию запуска и перечень технических отклонений.
- Обнови IMPLEMENTATION_STATUS.md фактическими результатами.

Готовность: нормальные сценарии работают, нет заглушек, ресурсы и зависимости доступны, нет жёстких путей с машины разработчика, проверки описаны честно.

## 6. Восстановленный код слайдов

Следующие листинги — эталон структуры и поведения, а не указание безусловно перезаписать файлы заготовки. Форматирование нормализовано. Nullable и совместимость пакетов адаптировать по разделу 4. Конструктор MainWindow приведён отдельно как техническое дополнение.


### 6.1. DataAccess/Provincia.cs — страница 10

```csharp
namespace DataAccess
{
    public class Provincia
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
    }
}
```


### 6.2. DataAccess/ProvinciasJSON.cs — страницы 11–13

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataAccess
{
    public class ProvinciasJSON
    {
        public string Path { get; set; }

        public ProvinciasJSON(string path)
        {
            Path = path;
        }

        public List<Provincia> getProvincias()
        {
            string jsonString = File.ReadAllText(Path);
            try
            {
                List<Provincia> provincias =
                    JsonConvert.DeserializeObject<List<Provincia>>(jsonString);
                return provincias;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public bool saveProvincias(List<Provincia> provincias)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(provincias);
                File.WriteAllText(Path, jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
```


### 6.3. UserInterface/ProvinciaUserControl.xaml — страница 15

```xml
<UserControl x:Class="UserInterface.ProvinciaUserControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:local="clr-namespace:UserInterface"
             mc:Ignorable="d" d:DesignWidth="350" Height="30">
    <Grid>
        <Label Content="ID" HorizontalAlignment="Left"
               VerticalAlignment="Top"/>
        <TextBox x:Name="idTextBox" HorizontalAlignment="Left"
                 Margin="22,4,0,0" TextWrapping="Wrap" Text=""
                 VerticalAlignment="Top" Width="50"
                 LostFocus="idTextBox_LostFocus"/>
        <Label Content="Nombre" HorizontalAlignment="Left"
               VerticalAlignment="Top" Margin="77,0,0,0"/>
        <TextBox x:Name="nombreTextBox" HorizontalAlignment="Left"
                 Margin="131,4,0,0" TextWrapping="Wrap" Text=""
                 VerticalAlignment="Top" Width="211"
                 LostFocus="nombreTextBox_LostFocus"/>
    </Grid>
</UserControl>
```


### 6.4. UserInterface/ProvinciaUserControl.xaml.cs — страницы 16–18

```csharp
using System;
using System.Windows;
using System.Windows.Controls;
using DataAccess;

namespace UserInterface
{
    public partial class ProvinciaUserControl : UserControl
    {
        private Provincia provincia;

        public Provincia Provincia
        {
            set
            {
                this.provincia = value;
                this.idTextBox.Text = value.ID.ToString();
                this.nombreTextBox.Text = value.Nombre;
            }
            get
            {
                return this.provincia;
            }
        }

        public ProvinciaUserControl()
        {
            InitializeComponent();
        }

        private void idTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                provincia.ID = Convert.ToInt32(idTextBox.Text);
            }
            catch (FormatException)
            {
                provincia.ID = 0;
                idTextBox.Text = "0";
            }
        }

        private void nombreTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            provincia.Nombre = nombreTextBox.Text;
        }
    }
}
```


### 6.5. UserInterface/MainWindow.xaml — страница 6

```xml
<Window x:Class="UserInterface.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:UserInterface"
        mc:Ignorable="d"
        Title="Gestión de Provincias" Height="450" Width="800">
    <Grid>
        <StackPanel x:Name="botonesStackPanel" Height="40"
                    Orientation="Horizontal" VerticalAlignment="Top">
            <Button x:Name="cargarButton" Content="Cargar"
                    Margin="10,10,10,10" Click="cargarButton_Click"/>
            <Button x:Name="idButton" Content="Ordenar por ID"
                    Margin="0,10,10,10" Click="idButton_Click" IsEnabled="False"/>
            <Button x:Name="nombreButton" Content="Ordenar por Nombre"
                    Margin="0,10,10,10" Click="nombreButton_Click" IsEnabled="False"/>
            <Button x:Name="guardarButton" Content="Guardar"
                    Margin="0,10,10,10" Click="guardarButton_Click" IsEnabled="False"/>
            <Button x:Name="guardarComoButton" Content="Guardar Como"
                    Margin="0,10,10,10" Click="guardarComoButton_Click" IsEnabled="False"/>
            <Button x:Name="exportarButton" Content="Exportar a PDF"
                    Margin="0,10,10,10" Click="exportarButton_Click" IsEnabled="False"/>
        </StackPanel>
        <ScrollViewer Margin="0,40,0,25">
            <StackPanel x:Name="provinciasStackPanel">
            </StackPanel>
        </ScrollViewer>
        <StatusBar Height="25" VerticalAlignment="Bottom">
            <StatusBarItem>
                <TextBlock Name="statusText" Text="Fichero no cargado"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```


### 6.6. UserInterface/MainWindow.xaml.cs — объединение страниц 19–25 и 30

```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using ExportPDF;
using Microsoft.Win32;

namespace UserInterface
{
    public partial class MainWindow : Window
    {
        private ProvinciasJSON provinciasJSON;
        private List<Provincia> provincias;

        // Техническое дополнение: тело конструктора на слайде свёрнуто.
        // Если Closing уже подключён в XAML/заготовке, вторую подписку не добавлять.
        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;
        }

        private void cargarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivo JSON|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                provinciasJSON = new ProvinciasJSON(openFileDialog.FileName);
                provincias = provinciasJSON.getProvincias();
                if (provincias != null)
                {
                    showProvincias();
                    activarBotones(true);
                    statusText.Text = openFileDialog.FileName;
                }
                else
                {
                    provinciasStackPanel.Children.Clear();
                    activarBotones(false);
                    MessageBox.Show("Error cargando datos", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    statusText.Text = "Error cargando datos";
                }
            }
        }

        private void showProvincias()
        {
            provinciasStackPanel.Children.Clear();
            foreach (Provincia provincia in provincias)
            {
                ProvinciaUserControl provinciaUserControl = new ProvinciaUserControl
                {
                    Provincia = provincia,
                };
                provinciasStackPanel.Children.Add(provinciaUserControl);
            }
        }

        private void activarBotones(bool activar)
        {
            foreach (Button b in botonesStackPanel.Children)
            {
                if (b.Name != "cargarButton")
                    b.IsEnabled = activar;
            }
        }

        private void orderProvincias(bool nombre)
        {
            if (nombre)
                provincias = provincias.OrderBy(o => o.Nombre).ToList();
            else
                provincias = provincias.OrderBy(o => o.ID).ToList();
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            orderProvincias(false);
            showProvincias();
        }

        private void nombreButton_Click(object sender, RoutedEventArgs e)
        {
            orderProvincias(true);
            showProvincias();
        }

        private bool guardar()
        {
            bool res = provinciasJSON.saveProvincias(provincias);
            if (!res)
            {
                MessageBox.Show("Error guardando datos", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                MessageBox.Show("Datos guardados correctamente en " + provinciasJSON.Path,
                    "Datos guardados", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
        }

        private void guardarButton_Click(object sender, RoutedEventArgs e)
        {
            guardar();
        }

        private void guardarComoButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo JSON|*.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                provinciasJSON.Path = saveFileDialog.FileName;
                if (provinciasJSON.saveProvincias(provincias))
                {
                    statusText.Text = saveFileDialog.FileName;
                    MessageBox.Show("Datos guardados correctamente en " + saveFileDialog.FileName,
                        "Datos guardados", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error guardando datos en " + saveFileDialog.FileName,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (provincias == null)
            {
                return;
            }
            switch (MessageBox.Show("¿Quiere guardar los datos antes de salir?",
                "Saliendo de la aplicación", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    if (guardar())
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                    break;
                case MessageBoxResult.No:
                    e.Cancel = false;
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void exportarButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo PDF|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                ProvinciasPDF provinciasPDF = new ProvinciasPDF();
                if (provinciasPDF.SavePDF(provincias, saveFileDialog.FileName))
                {
                    if (MessageBox.Show("Datos exportados a PDF en " + saveFileDialog.FileName
                        + ".\n\n ¿Desea abrirlo?", "Exportación correcta",
                        MessageBoxButton.YesNo, MessageBoxImage.Information)
                        == MessageBoxResult.Yes)
                    {
                        // Process.Start(saveFileDialog.FileName);
                        var p = new Process();
                        p.StartInfo = new ProcessStartInfo(saveFileDialog.FileName)
                        {
                            UseShellExecute = true
                        };
                        p.Start();
                    }
                }
            }
        }
    }
}
```


### 6.7. ExportPDF/ProvinciasPDF.cs — страницы 28–29

```csharp
using System;
using System.Collections.Generic;
using DataAccess;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;

namespace ExportPDF
{
    public class ProvinciasPDF
    {
        public bool SavePDF(List<Provincia> provincias, string path)
        {
            try
            {
                if (PdfSharp.Capabilities.Build.IsCoreBuild)
                    GlobalFontSettings.FontResolver = new FailsafeFontResolver();

                Document document = new Document();
                Section section = document.AddSection();
                Paragraph paragraph = section.AddParagraph();
                paragraph.AddText("Provincias");
                paragraph.Format.Font.Size = 20;
                paragraph.Format.Font.Bold = true;
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                section.AddParagraph();

                Table table = section.AddTable();
                table.Style = "Table";

                Column column = table.AddColumn("2cm");
                column.Format.Alignment = ParagraphAlignment.Center;
                column = table.AddColumn("10cm");
                column.Format.Alignment = ParagraphAlignment.Left;

                Row row = table.AddRow();
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
                row.Shading.Color = Colors.LightBlue;
                row.Cells[0].AddParagraph("ID").Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph("Provincia");

                foreach (Provincia p in provincias)
                {
                    row = table.AddRow();
                    row.Cells[0].AddParagraph(p.ID.ToString()).Format.Alignment =
                        ParagraphAlignment.Center;
                    row.Cells[1].AddParagraph(p.Nombre);
                }

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer
                {
                    Document = document,
                    PdfDocument = new PdfDocument()
                };
                pdfRenderer.RenderDocument();
                pdfRenderer.PdfDocument.Save(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
```


## 7. Финальная матрица приёмки

Это проверочный список по базовому примеру, не официальная балльная рубрика. Отмечай «пройдено» только после фактической проверки. Сборка не заменяет GUI-проверку.

| ID | Сценарий | Ожидаемый результат |
|---|---|---|
| B01 | Сборка всего решения | нет ошибок компиляции/XAML, зависимости разрешены |
| B02 | Запуск | пустой список, Fichero no cargado, доступна только Cargar |
| B03 | Закрыть сразу после запуска | нет запроса сохранения |
| B04 | Отменить диалог загрузки | состояние не меняется |
| B05 | Открыть предоставленный JSON | все элементы отображаются, поля верны, путь в статусе |
| B06 | Проверить кнопки после загрузки | все шесть доступны |
| B07 | Прокрутить список | доступны все провинции, строка состояния остаётся внизу |
| B08 | Загрузить другой валидный JSON | старые строки заменены, не продублированы |
| B09 | Открыть синтаксически сломанный JSON | при обработанном JsonReaderException список очищен, пять кнопок отключены, ошибка в диалоге и статусе |
| B10 | Загрузить [] | пустой список, но успешная загрузка и включённые кнопки |
| B11 | Изменить ID и нажать Tab | новое число в модели |
| B12 | Ввести буквы в ID и нажать Tab | ID=0 в модели и поле |
| B13 | Изменить Nombre и нажать Tab | имя обновлено в модели |
| B14 | После изменения нажать сортировку | изменения сохранены в объектах, строки перестроены |
| B15 | Сортировать по ID | числовой возрастающий порядок |
| B16 | Сортировать по Nombre | порядок OrderBy по строковому свойству |
| B17 | Сохранить копию с изменениями | JSON содержит текущие значения и читается обратно |
| B18 | Save As, затем обычный Save | используется новый путь, статус соответствует успешному Save As |
| B19 | Отменить Save As | путь и данные не меняются |
| B20 | Закрыть с данными, Cancel | окно остаётся открытым |
| B21 | Закрыть с данными, No | окно закрывается без записи |
| B22 | Закрыть с данными, Yes | сохранить и закрыть при успехе |
| B23 | Ошибка сохранения при закрытии с Yes | сообщение ошибки, окно остаётся открытым |
| B24 | Отменить диалог PDF | файл не создаётся |
| B25 | Экспорт PDF | файл существует, открывается, содержит заголовок и таблицу |
| B26 | Проверить PDF | ID/Provincia, ширины и выравнивание, голубая жирная шапка |
| B27 | Экспорт после редактирования и сортировки | текущие значения и текущий порядок, а не исходный JSON |
| B28 | Имена Córdoba / Jaén / Málaga | испанские символы читаемы в UI, JSON и PDF |
| B29 | Ответ No на предложение открыть PDF | документ сохранён, открытие не запускается |
| B30 | Ответ Yes на предложение открыть PDF | запускается открытие файла |
| B31 | Повторить экспорт в той же сессии | второй документ также создаётся |
| B32 | Закрытие после сохранения без новых правок | вопрос всё равно появляется, как в учебном примере |
| B33 | Проверить исходники | нет временных заглушек, жёстких путей, лишних расширений |

Дополнительные интеграционные проверки: редактировать поле и сразу нажать Guardar/Exportar/закрыть окно, не нажимая предварительно Tab. Убедиться, что фактическая последовательность фокуса не теряет последнее изменение. Если возникает проблема, исправить минимально и описать изменение, не добавляя новую архитектуру.

Операции записи выполняй на тестовых копиях. Недоступный путь для проверки ошибки выбирай безопасно: не меняй права системных каталогов.

## 8. Что должно остаться в готовом проекте

- Решение и исходные файлы всех трёх проектов (либо согласованная эквивалентная структура заготовки).
- Корректные ссылки между проектами и PackageReference.
- Предоставленный файл данных/ресурсы, если они входят в заготовку.
- Краткая инструкция: требования окружения, как собрать, что запускать, как выбрать JSON.
- Список фактических результатов проверок и технических отклонений.

Не утверждай, что ZIP, отчёт, видео, скриншоты или конкретное имя архива обязательны: в разобранном материале этих правил сдачи нет. Пример PDF и инструкция полезны для проверки, но не объявляются официальным требованием преподавателя.

## 9. Готовые команды-задания для агента

Перед первым запуском помести этот план рядом с решением и предоставь агенту доступ к PDF и заготовке. В следующих командах «план» означает этот документ, независимо от его имени.

### Первый запуск

```text
Прочитай план реализации Taller 6 и инструкции репозитория. Выполни ТОЛЬКО этап 0: аудит существующей заготовки и сопоставление с базовым заданием PDF. Не реализуй функции и не пересоздавай решение. Укажи фактические проекты, target frameworks, зависимости, результат исходной сборки и минимальный план изменений. Создай IMPLEMENTATION_STATUS.md. Дополнительные задания страницы 31 не выполнять.
```

### Последующие запуски

```text
Прочитай план и IMPLEMENTATION_STATUS.md. Выполни ТОЛЬКО этап N. Сначала проверь состояние файлов. Используй восстановленный код как образец, не переписывай рабочие части без причины. Не выполняй дополнительные задания. Проверь этап, обнови журнал, сообщи изменённые файлы, результаты и ограничения. Не начинай следующий этап.
```

Подставляй N = 1, 2, 3, 4, 5 последовательно.

### Финальная проверка

```text
Выполни только этап 6. Сопоставь фактический проект с разделами 1–3 и матрицей раздела 7. Исправь только обнаруженные несоответствия базовому заданию. Не добавляй улучшения страницы 31. Проверь сборку, отсутствие заглушек, подключение Closing ровно один раз, Save As, сохранение последних изменений полей, повторный экспорт PDF. Отдели выполненные тесты от непроверенных из-за окружения. Выдай итоговый список готовности и минимальную инструкцию запуска.
```

## 10. Карта источников внутри PDF

| Страницы | Назначение |
|---|---|
| 2 | внешний вид работающего приложения |
| 3–4 | создание UserInterface, настройки подробно не расшифрованы |
| 5–6 | начальное окно и MainWindow.xaml |
| 7–8 | создание DataAccess и Newtonsoft.Json |
| 9 | исходный JSON, предоставлен отдельно |
| 10 | Provincia.cs |
| 11–13 | ProvinciasJSON.cs |
| 14–15 | создание компонента и его XAML |
| 16–18 | модель компонента и LostFocus |
| 19 | структура MainWindow, конструктор свёрнут |
| 20–21 | загрузка, показ, доступность кнопок |
| 22–23 | сохранение и Save As |
| 24 | закрытие окна |
| 25 | сортировка |
| 26–27 | создание ExportPDF и пакет PDF |
| 28–29 | ProvinciasPDF.cs |
| 30 | обработчик экспорта и открытие PDF |
| 31 | дополнительные задания — исключены из текущего объёма |

Примечание: имена файлов в примерах выше — логические пути внутри решения. Реальный путь бери из аудита, не создавай вложенную копию проекта из-за отличия структуры каталогов.
