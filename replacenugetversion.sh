find . -name *.nuspec -maxdepth 2 -exec sed -i "s/$1/$2/" {} \;
