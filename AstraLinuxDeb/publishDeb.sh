#!/bin/bash
set -euo pipefail
umask 022

package_file=$(realpath "${1:?Укажите файл .deb}")
repo_dir=${2:?Укажите каталог репозитория}
: "${GNUPGHOME:?Укажите хранилище ключа подписи репозитория}"

package=$(dpkg-deb --field "$package_file" Package)
version=$(dpkg-deb --field "$package_file" Version)
architecture=$(dpkg-deb --field "$package_file" Architecture)
if [[ "$package" != qs-workwear || "$architecture" != amd64 ]]; then
    echo 'Для этого репозитория нужен пакет qs-workwear для amd64.' >&2
    exit 1
fi

install -d -m 755 "$repo_dir"
cd "$repo_dir"
exec 9>.publish.lock
flock 9

metadata=$(mktemp -d .publish.XXXXXXXX)
trap 'rm -rf -- "$metadata"' EXIT
gpg --batch --export > "$metadata/keyring.gpg"
test -s "$metadata/keyring.gpg"

package_name="${package}_${version}_${architecture}.deb"
if [[ -e "$package_name" ]]; then
    if ! cmp -s "$package_file" "$package_name"; then
        echo "Версия $version уже опубликована с другим содержимым." >&2
        exit 1
    fi
else
    install -m 644 "$package_file" "$metadata/$package_name"
    mv "$metadata/$package_name" "$package_name"
fi

apt-ftparchive packages . > "$metadata/Packages"
gzip -n -c "$metadata/Packages" > "$metadata/Packages.gz"
apt-ftparchive \
    -o APT::FTPArchive::Release::Origin='Quality Solution' \
    -o APT::FTPArchive::Release::Label='QS: Workwear' \
    -o APT::FTPArchive::Release::Architectures=amd64 \
    -o APT::FTPArchive::Release::Acquire-By-Hash=yes \
    release "$metadata" > "$metadata/Release.tmp"
gpg --batch --clearsign --output "$metadata/InRelease" "$metadata/Release.tmp"

# Клиенты со старым InRelease могут дочитать прежний индекс по его хэшу.
for algorithm in SHA256 SHA512; do
    install -d "by-hash/$algorithm"
    for index in Packages Packages.gz; do
        hash=$("${algorithm,,}sum" "$metadata/$index")
        ln -f "$metadata/$index" "by-hash/$algorithm/${hash%% *}"
    done
done

mv "$metadata/Packages" "$metadata/Packages.gz" "$metadata/keyring.gpg" .
mv "$metadata/InRelease" .
ln -sfn "${PWD##*/}/$package_name" ../qs-workwear-latest.deb
