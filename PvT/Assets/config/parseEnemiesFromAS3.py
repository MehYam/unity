import fileinput
import re

planeNames = []

f = open('c:\\source\\flash\\game1\\src\\ActorAssetManager.as', 'r')
for plane in f:
	if 'symbol=\"ship' in plane:
		name = re.search(r'"ship.*"', plane)
		#print(name.group().replace('"', ''))
		planeNames.append(name.group())

print("{")
for line in fileinput.input():
	if "new PlaneEnemyEnum" in line:
		#print(line)

		#trim the line
		start = line.index("ActorAttrs");
		line = line[start:]

		#extract the name
		name = re.search(r'\".+\"', line).group()
		#print(name.group())

		line = line.replace(name, "")

		#extract the numerical args, some have decimals 
		numbers = re.findall(r'\d+\.\d+|\d+', line)
		#print(numbers, len(numbers))

		#dump the json
		print('\t%s:' % name)
		print('\t{')
		print('\t\t"asset": %s,' % planeNames[int(numbers[6])])
		print('\t\t"health": "%s",' % numbers[0])
		print('\t\t"maxSpeed": "%s",' % numbers[1])
		print('\t\t"acceleration": "%s",' % numbers[2])
		print('\t\t"inertia": "%s",' % numbers[3])
		print('\t\t"collision": "%s",' % numbers[5])
		print('\t\t"reward": "%s",' % numbers[7])
		print('\t\t"behaviors":')
		print('\t\t[')
		print('\t\t]')
		print('\t},')

print("}")