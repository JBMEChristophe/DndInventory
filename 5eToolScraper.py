# coding: utf-8
# -*- coding: utf-8 -*-

import csv
import argparse
import urllib.request
import json
import time
import asyncio
from aiohttp import ClientSession, ClientResponseError
from pathlib import Path
import xml.etree.ElementTree as ET
from PIL import Image
import io

import nest_asyncio
nest_asyncio.apply()

import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

default_image_base_path = "DNDinventory/Images/Items"
image_download_base = "https://5e.tools/img/items"
items_download_json = "https://5e.tools/data/items.json"
item_base_url = "https://5e.tools/items.html#"

class Item:
    def __init__(self):
        self.Name = None
        self.Type = None
        self.Cost = None
        self.Weight = None
        self.Source = None
        self.Rarity = None
        self.Attunement = None
        self.Url = None

    def __repr__(self):
        return str(self)
    def __str__(self):
        return "{0}, {1}, {2}, {3}, {4}".format(self.Name, self.Type, self.Cost, self.Weight, self.Source)

# Print iterations progress
def printProgressBar (iteration, total, prefix = '', suffix = '', decimals = 1, length = 100, fill = 'O', printEnd = "\r"):
    """
    Call in a loop to create terminal progress bar
    @params:
        iteration   - Required  : current iteration (Int)
        total       - Required  : total iterations (Int)
        prefix      - Optional  : prefix string (Str)
        suffix      - Optional  : suffix string (Str)
        decimals    - Optional  : positive number of decimals in percent complete (Int)
        length      - Optional  : character length of bar (Int)
        fill        - Optional  : bar fill character (Str)
        printEnd    - Optional  : end character (e.g. "\r", "\r\n") (Str)
    """
    percent = ("{0:." + str(decimals) + "f}").format(100 * (iteration / float(total)))
    filledLength = int(length * iteration // total)
    bar = fill * filledLength + '-' * (length - filledLength)
    print(f'\r{prefix} |{bar}| {percent}% {suffix}', end = printEnd)
    # Print New Line on Complete
    if iteration == total: 
        print()

def indent(elem, level=0, hor='\t', ver='\n'):
    i = ver + level * hor
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + hor
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
        for elem in elem:
            indent(elem, level + 1, hor, ver)
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i
            
def create_xml(export_list):
    id = 0
    items = ET.Element("ArrayOfCatalogItemModel")
    for obj in export_list:
        item = ET.SubElement(items,"CatalogItemModel")
        ID = ET.SubElement(item,"ID")
        ID.text = "{0}_{1}".format(obj.Source, obj.Name.replace(" ","_"))
        Name = ET.SubElement(item,"Name")
        Name.text = obj.Name
        Type = ET.SubElement(item,"Type")
        if(obj.Type is not None):
            for t in obj.Type:
                ItemType = ET.SubElement(Type,"ItemType")
                ItemType.text = t
        Rarity = ET.SubElement(item,"Rarity")
        Rarity.text = obj.Rarity
        Attunement = ET.SubElement(item,"Attunement")
        Attunement.text = obj.Attunement
        Properties = ET.SubElement(item,"Properties")
        Properties.text = obj.Properties
        Description = ET.SubElement(item,"Description")
        Description.text = obj.Description
        Cost = ET.SubElement(item,"Cost")
        Cost.text = obj.Cost
        Weight = ET.SubElement(item,"Weight")
        Weight.text = obj.Weight
        Source = ET.SubElement(item,"Source")
        Source.text = obj.Source
        IsStackable = ET.SubElement(item,"IsStackable")
        IsStackable.text = "false"
        CellSpanX = ET.SubElement(item,"CellSpanX")
        CellSpanX.text = "1"
        CellSpanY = ET.SubElement(item,"CellSpanY")
        CellSpanY.text = "1"
        id += 1
            
    indent(items)
    tree = ET.ElementTree(items)
    tree.write("Items.xml",encoding='utf-8', xml_declaration=True)

async def fetch(session, url):
    try:
        async with session.get(url, timeout=15) as response:
            resp = await response.read()
    except ClientResponseError as e:
        print(e.code)
    except asyncio.TimeoutError:
        print("asyncio Timeout")
    except Exception as e:
        print(e)
    else:
        return resp
    return

async def fetch_async(urls):
    tasks = []
    # try to use one client session
    async with ClientSession() as session:
        for i in range(len(urls)):
            task = asyncio.ensure_future(fetch(session, urls[i]))
            tasks.append(task)
        # await response outside the for loop
        responses = await asyncio.gather(*tasks)
    return responses

async def save_image(path, response):
    p = Path(path)
    Path(p.parent).mkdir(parents=True, exist_ok=True)
    if(not p.exists()):
        decode = ""
        try:
            decode = response.decode("utf-8")
        except UnicodeDecodeError:
            pass

        if "404" not in decode:            
            image = Image.open(io.BytesIO(response))
            image.save(path)

async def save_images_async(paths, responses):
    tasks = []
    # try to use one client session
    for i in range(len(responses)):
        task = asyncio.ensure_future(save_image(paths[i], responses[i]))
        tasks.append(task)
    # await response outside the for loop
    responses = await asyncio.gather(*tasks)
    return responses

async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', '--item_path', default='Items.json', help='Item input path')
    parser.add_argument('-img', '--image_path', default=default_image_base_path, help='Output item image path')
    args = parser.parse_args()

    print("Downloading items list")
    req = urllib.request.Request(items_download_json, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req) as url:
        data = json.load(url)
        with open(args.item_path, 'w', encoding='utf8') as f:
            f.write(json.dumps(data, indent=4))

    items = data["item"]

    print("Parsing item data")
    export_items = []
    downloads = []
    l = len(items)
    printProgressBar(0, l, prefix = 'Progress:', suffix = 'Complete', length = 50)
    for index, item in enumerate(items):
        export_item = Item()
        for key, value in item.items():
            if key not in {"name", "type", "value", "weight", "source", "rarity", "reqAttune"}:
                continue

            if(value is str and ('â€”' in value or 'Ã—' in value)):
                value = value.replace('â€”','')
                value = value.replace('Ã—','x')
                
            if key == 'name':                
                export_item.Name = value
                printProgressBar(index, l, prefix = 'Progress:', suffix = export_item.Name, length = 50)
            elif key == 'type':
                tmp = [] 
                if(value != ""):               
                    for t in value.split(', '):
                        firstDelPos=t.find("(") # get the position of (
                        secondDelPos=t.find(")") # get the position of )
                        typeItem = t.replace(t[firstDelPos:secondDelPos+1], "") # remove the string between two delimiters
                        typeItem = typeItem.replace('\'', '')
                        typeItem = ''.join(x for x in typeItem.title() if not x.isspace())
                        tmp.append(typeItem)
                    export_item.Type = tmp
                else:
                    export_item.Type = None
            elif key == 'value':
                export_item.Cost = value
            elif key == 'weight':
                export_item.Weight = value
            elif key == 'source':
                export_item.Source = value
            elif key == 'rarity':
                export_item.Rarity = value
            elif key == 'reqAttune':
                export_item.Attunement = value

        item_web_name = str.replace(export_item.Name, " ", "%20")
        export_item.Url = f"{item_base_url}{item_web_name}_{export_item.Source}"
        export_items.append(export_item)
        save_path = f"{args.image_path}/{export_item.Name}.jpg"
        image_url = f"{image_download_base}/{export_item.Source}/{export_item.Name}.jpg"
        downloads.append((save_path, image_url))
    printProgressBar(l, l, prefix = 'Progress:', suffix = 'Complete', length = 50)

    print("Create xml file")
    create_xml(export_items)

    loop = asyncio.get_event_loop()

    print("Download item image data")
    future = asyncio.ensure_future(fetch_async([url for (path, url) in downloads]))
    loop.run_until_complete(future)
    responses = future.result()

    print("Save image data to file")
    future = asyncio.ensure_future(save_images_async([path for (path, url) in downloads], responses))
    loop.run_until_complete(future)

    print("Done")

asyncio.run(main())
