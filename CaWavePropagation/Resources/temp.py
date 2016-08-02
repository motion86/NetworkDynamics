# -*- coding: utf-8 -*-

import matplotlib.pyplot as plt
import csv
import os

plt.rc('lines',linewidth=2)
plt.rc('font',size=20)
plt.rc('axes',color_cycle=[plt.get_cmap('Set2')(int(i/7*255)) for i in range(8)])

plt.xlabel('$t$')
plt.ylabel('$Count$')

# read from CSV file
name = "";
myFile = ["temp.txt","temp1.txt"]
green_xdata = []; purple_xdata = []; red_xdata = [];
green_ydata = []; purple_ydata = []; red_ydata = [];
for k,f in enumerate(myFile):
    my_x = []; my_y = []; my_color = [];
    with open(f, "r") as fh:
      # uses tabs instead of commas for reader
      reader = csv.reader(fh, delimiter="\t")
      # read each row and print to screen
      for i, row in enumerate(reader):
          if(i == 0): name = row[0]
          elif(i == 1): path = row[0]
          else:
              my_x.append(row[0])
              my_y.append(row[1])
              my_color.append(row[2])
              #print("sin("+row[0]+") = "+row[1])

      #plt.subplot(3,1,1)
      if not os.path.exists(path):
          os.makedirs(path)
      if k > 0:
         for j, c in enumerate(my_color):
             print(c)
             if c == "Green":
                green_xdata.append(my_x[j])
                green_ydata.append(my_y[j])
             if c == "Purple":
                purple_xdata.append(my_x[j])
                purple_ydata.append(my_y[j])
             if c == "Red":
                red_xdata.append(my_x[j])
                red_ydata.append(my_y[j])
         for j in range(3):
             if j == 0:
                 plt.plot(green_xdata, green_ydata, "og", label="$ top10 node$")
             if j == 1:
                 plt.plot(purple_xdata,purple_ydata, "om", label="$ t10 + top$")
             if j == 2:
                 #print(red_data)
                 plt.plot(red_xdata,red_ydata, "or", label="$ top-node$")
      else:
           plt.plot(my_x,my_y, "k")

plt.title("$" + name[:6] + "$")
plt.legend(loc='best',fontsize='small',frameon=False, columnspacing=1, handlelength=1, handletextpad=0)
plt.tight_layout()
plt.savefig(path + name + ".pdf")
plt.close('all')
      
      #print(path + name + ".pdf")