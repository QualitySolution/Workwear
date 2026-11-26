%define appname qs-workwear
%define instdir %{_libdir}/%{appname}

Name:       %{appname}
Version:    %{ver}
Release:    alt%{rel}
Summary:    Программа учёта спецодежды QS: Спецодежда
License:    Proprietary
Group:      Office
URL:        https://workwear.qsolution.ru/
Source0:    %{appname}-%{version}.tar.gz
Source1:    workwear.desktop
Source2:    logo.png

Requires:   mono-core
Requires:   mono-data
Requires:   libgtk-sharp2
Requires:   libgdiplus
Requires:   fonts-ttf-dejavu

%description
Программа позволяет вести учёт спецодежды, выдаваемой сотрудникам предприятия.
Разработана компанией Quality Solution.

%prep
%setup -q

%build
# Бинарные файлы уже собраны заранее, компиляция не требуется.

%install
mkdir -p %{buildroot}%{instdir}
cp -r * %{buildroot}%{instdir}/

# Лаунчер скрипт
mkdir -p %{buildroot}%{_bindir}
printf '#!/bin/bash\ncd %{instdir}\nexec mono %{instdir}/workwear.exe "$@"\n' \
    > %{buildroot}%{_bindir}/%{appname}
chmod 755 %{buildroot}%{_bindir}/%{appname}

# Иконка
install -D -m644 %{SOURCE2} %{buildroot}%{_datadir}/pixmaps/%{appname}.png

# Ярлык в меню приложений
mkdir -p %{buildroot}%{_datadir}/applications
cp %{SOURCE1} %{buildroot}%{_datadir}/applications/%{appname}.desktop

%files
%{instdir}/
%{_bindir}/%{appname}
%{_datadir}/pixmaps/%{appname}.png
%{_datadir}/applications/%{appname}.desktop

%changelog
* %{builddate} Quality Solution <support@qsolution.ru> %{ver}-alt%{rel}
- Сборка версии %{ver}

