# Правила редактирования GTK GUI

## Проблема

В проекте используется Stetic (Mono/GTK# дизайнер форм). Стетик хранит описание интерфейса
в **двух** связанных файлах:

1. **`gtk-gui/gui.stetic`** — исходный файл дизайнера, который редактируется вручную или через GUI-редактор (Stetic/MonoDevelop).
2. **`gtk-gui/Workwear.<Namespace>.<ViewName>.cs`** — **автогенерируемый** C#-файл, создаётся инструментом `stetic` на основе `gui.stetic`.

## Важное правило

> **При внесении изменений в интерфейс GTK необходимо одновременно вносить правки как в автогенерируемый `*.cs` файл, так и в исходный `gui.stetic` файл.**

Если обновить только `*.cs` файл — при следующей регенерации из `gui.stetic` изменения будут **перезаписаны и утеряны**.

## Пример: добавление нового виджета

Допустим, добавляется `yCheckButton` с именем `ycheckMyOption` во `VBox` на позицию 4 (кнопка Run сдвигается на позицию 5).

### 1. Автогенерируемый файл (`gtk-gui/Workwear.MyNamespace.MyView.cs`)

```csharp
// Добавить объявление поля:
private Gamma.GtkWidgets.yCheckButton ycheckMyOption;

// В методе Build() добавить создание виджета:
Gamma.GtkWidgets.yCheckButton w8 = new Gamma.GtkWidgets.yCheckButton();
w8.CanFocus = true;
w8.Name = "ycheckMyOption";
w8.Label = global::Mono.Unix.Catalog.GetString("Мой параметр");
w8.DrawIndicator = true;
w8.UseUnderline = true;
vbox1.Add(w8);
Box.BoxChild w8c = ((Box.BoxChild)(vbox1[w8]));
w8c.Position = 4;
w8c.Expand = false;
w8c.Fill = false;
this.ycheckMyOption = w8;

// Обновить позицию кнопки Run (была 4, стала 5):
w9c.Position = 5;
```

### 2. Файл дизайнера (`gtk-gui/gui.stetic`)

Найти секцию нужного виджета (`<widget class="Gtk.Bin" id="Workwear.MyNamespace.MyView">`)
и добавить новый `<child>` **перед** `<child>` с `buttonRun`:

```xml
<child>
  <widget class="Gamma.GtkWidgets.yCheckButton" id="ycheckMyOption">
    <property name="MemberName" />
    <property name="CanFocus">True</property>
    <property name="Label" translatable="yes">Мой параметр</property>
    <property name="DrawIndicator">True</property>
    <property name="HasLabel">True</property>
    <property name="UseUnderline">True</property>
  </widget>
  <packing>
    <property name="Position">4</property>
    <property name="AutoSize">True</property>
    <property name="Expand">False</property>
    <property name="Fill">False</property>
  </packing>
</child>
```

И обновить позицию кнопки Run:

```xml
<packing>
  <property name="PackType">End</property>
  <property name="Position">5</property>  <!-- было 4 -->
  ...
</packing>
```

## Где искать нужную секцию в `gui.stetic`

Файл `gui.stetic` очень большой. Найти нужную секцию:

```bash
grep -n "MyView" gtk-gui/gui.stetic
```

Затем прочитать нужный диапазон строк через `sed`:

```bash
sed -n '33709,33820p' gtk-gui/gui.stetic
```

Так как файл слишком большой для прямого редактирования инструментами IDE, используйте `sed -i` для точечных вставок и замен по номеру строки.

