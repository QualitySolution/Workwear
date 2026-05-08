#!/bin/bash
set -e

VERSION=${1:?"Укажите версию как первый аргумент: makeRpm.sh <version> [release]"}
RELEASE=${2:-1}
WORKSPACE=/workspace
BUILD_DATE=$(date +"%a %b %d %Y")
HOME=/home/builder

echo "=== Сборка RPM для AltLinux: версия ${VERSION}-alt${RELEASE} ==="

# Создаём дерево каталогов rpmbuild (на AltLinux по умолчанию ~/RPM/)
mkdir -p ~/RPM/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

# Копируем spec файл
cp "${WORKSPACE}/AltLinuxRpm/workwear.spec" ~/RPM/SPECS/

# Создаём архив из собранных Linux-бинарников
SRC_DIR="/tmp/qs-workwear-${VERSION}"
rm -rf "${SRC_DIR}"
mkdir -p "${SRC_DIR}"
cp -r "${WORKSPACE}/Workwear/bin/Release/"* "${SRC_DIR}/"

tar czf ~/RPM/SOURCES/qs-workwear-${VERSION}.tar.gz \
    -C /tmp "qs-workwear-${VERSION}"

# Копируем .desktop файл в SOURCES чтобы spec мог его достать
cp "${WORKSPACE}/AltLinuxRpm/workwear.desktop" ~/RPM/SOURCES/

echo "=== Запуск rpmbuild ==="
rpmbuild -bb ~/RPM/SPECS/workwear.spec \
    --define "ver ${VERSION}" \
    --define "rel ${RELEASE}" \
    --define "builddate ${BUILD_DATE}"

# Копируем готовый RPM обратно в workspace
echo "=== Копируем RPM в workspace ==="
find ~/RPM/RPMS -name "*.rpm" -exec cp {} "${WORKSPACE}/AltLinuxRpm/" \;

echo "=== Готово ==="
ls -lh "${WORKSPACE}/AltLinuxRpm/"*.rpm

