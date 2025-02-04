# Задание:

Вывести в выходной файл или на консоль описание протекающих в программе параллельных потоков (оператор параллельной композиции, а также алфавит, префиксную форму и протокол работы каждого потока). Если задание выполнять на основе ЛР, там все необходимые потоки и ресурсы уже имеются, и нужно лишь дополнить вывод программы требуемыми данными. Если писать программу с нуля, необходимо смоделировать несколько ресурсов и потоков, которые за них конкурируют. При этом описание потоков должно быть динамическим. Для этого необходимо, чтобы, во-первых, число потоков было настраиваемым (чтобы его можно было варьировать через входной файл или настройку в коде программы). Во-вторых, чтобы настраиваемым было количество ресурсов, и разным потокам требовалось разное количество разных ресурсов. В таком случае, и оператор параллельной композиции, и алфавит каждого потока, и его префиксная форма, и протокол - всё это будет динамически меняться в зависимости от настроек.
