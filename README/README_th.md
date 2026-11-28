<p align="center"><img src="https://github.com/IridiumIO/CompactGUI/assets/1491536/64f66b5d-0710-4f66-8b88-6a69f7eb9b63" width="500"></p>

<p align="center">
  <a href="https://github.com/IridiumIO/CompactGUI/releases">
    <img alt="ยอดดาวน์โหลด GitHub (ทุกไฟล์ ทุกเวอร์ชัน)" src="https://img.shields.io/github/downloads/IridiumIO/CompactGUI/total?style=for-the-badge&logo=github"></a>&nbsp;&nbsp;&nbsp;&nbsp;
  
  <a href="https://github.com/IridiumIO/CompactGUI/releases">
    <img alt="เวอร์ชัน GitHub" src="https://img.shields.io/github/v/release/IridiumIO/CompactGUI?style=for-the-badge"></a>
  <br/> 
</p>


<p align="center"><b>CompactGUI บีบอัดเกมและโปรแกรมของคุณแบบโปร่งใส ทำให้ใช้พื้นที่น้อยลงโดยยังใช้งานได้ตามปกติทุกอย่าง โดยทำงานผ่าน Win32 API โดยตรง ใช้กลไกการบีบอัดชุดเดียวกับเครื่องมือบรรทัดคำสั่ง <code>compact.exe</code> ที่มากับ Windows 10 เป็นต้นไป</b></p>

&nbsp;
&nbsp;

<p align="center"><img src="CompactGUI/assets/Home.png" width="750"/></p>
<p align="center">
<img src="CompactGUI/assets/Compression.png" width="250"/>
<img src="CompactGUI/assets/Database.png" width="250"/>
<img src="CompactGUI/assets/Watcher.png" width="250"/>
</p>

---


<p align="center">
  <a href="README/"><img src="https://img.shields.io/badge/README-%E0%B8%A0%E0%B8%B2%E0%B8%A9%E0%B8%B2%E0%B8%AD%E0%B8%B7%E0%B8%B9%E0%B9%88%E0%B9%80%E0%B8%99-606070?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIGhlaWdodD0iNDhweCIgdmlld0JveD0iMCAtOTYwIDk2MCA5NjAiIHdpZHRoPSI0OHB4IiBmaWxsPSIjYTFlMWZmIj48cGF0aCBkPSJNMzIzLTExMS41UTI1MC0xNDMgMTk2LTE5N3QtODUtMTI3LjVRODAtMzk4IDgwLTQ4MnQzMS0xNTYuNVExNDItNzExIDE5Ni03NjV0MTI3LTg0LjVRMzk2LTg4MCA0ODAtODgwdDE1NyAzMC41UTcxMC04MTkgNzY0LTc2NXQ4NSAxMjYuNVE4ODAtNTY2IDg4MC00ODJ0LTMxIDE1Ny41UTgxOC0yNTEgNzY0LTE5N3QtMTI3IDg1LjVRNTY0LTgwIDQ4MC04MHQtMTU3LTMxLjVaTTQ4MC0xMzhxMzUtMzYgNTguNS04Mi41VDU3Ny0zMzFIMzg0cTE0IDYwIDM3LjUgMTA4dDU4LjUgODVabS04NS0xMnEtMjUtMzgtNDMtODJ0LTMwLTk5SDE3MnEzOCA3MSA4OCAxMTEuNVQzOTUtMTUwWm0xNzEtMXE3Mi0yMyAxMjkuNS02OVQ3ODgtMzMxSDYzOXEtMTMgNTQtMzAuNSA5OFQ1NjYtMTUxWk0xNTItMzkxaDE1OXEtMy0yNy0zLjUtNDguNVQzMDctNDgycTAtMjUgMS00NC41dDQtNDMuNUgxNTJxLTcgMjQtOS41IDQzdC0yLjUgNDVxMCAyNiAyLjUgNDYuNVQxNTItMzkxWm0yMjEgMGgyMTVxNC0zMSA1LTUwLjV0MS00MC41cTAtMjAtMS0zOC41dC01LTQ5LjVIMzczcS00IDMxLTUgNDkuNXQtMSAzOC41cTAgMjEgMSA0MC41dDUgNTAuNVptMjc1IDBoMTYwcTctMjQgOS41LTQ0LjVUODIwLTQ4MnEwLTI2LTIuNS00NXQtOS41LTQzSDY0OXEzIDM1IDQgNTMuNXQxIDM0LjVxMCAyMi0xLjUgNDEuNVQ2NDgtMzkxWm0tMTAtMjM5aDE1MHEtMzMtNjktOTAuNS0xMTVUNTY1LTgxMHEyNSAzNyA0Mi41IDgwVDYzOC02MzBabS0yNTQgMGgxOTRxLTExLTUzLTM3LTEwMi41VDQ4MC04MjBxLTMyIDI3LTU0IDcxdC00MiAxMTlabS0yMTIgMGgxNTFxMTEtNTQgMjgtOTYuNXQ0My04Mi41cS03NSAxOS0xMzEgNjR0LTkxIDExNVoiLz48L3N2Zz4="/></a>
</p>


&nbsp;

**วิธีทำงาน:**

CompactGUI เป็นอินเทอร์เฟซที่ใช้งานง่ายซึ่งช่วยให้คุณใช้ประโยชน์จากการบีบอัดระบบไฟล์ที่ Windows เปิดให้ใช้ผ่าน Windows Overlay Filter (WOF) Driver โดยรองรับอัลกอริทึมบีบอัดประสิทธิภาพสูงที่เริ่มมีตั้งแต่ Windows 10 ช่วยให้คุณบีบอัดไฟล์หรือโฟลเดอร์ใด ๆ ก็ได้ (เน้นไปที่เกม) แบบโปร่งใส โดยไม่ทำให้ประสิทธิภาพลดลงและประหยัดพื้นที่จัดเก็บได้มาก

**โปร่งใส? หมายความว่ายังไง?**

การบีบอัดแบบโปร่งใสหมายความว่าไฟล์ยังคงใช้งานได้ตามปกติเหมือนไม่มีอะไรเกิดขึ้น ไฟล์จะไม่ถูกแพ็คใหม่แบบไฟล์ Zip หรือ Rar คุณยังเปิดดู เปิดเกม และเปิดโปรแกรมได้เหมือนเดิมทุกอย่าง เพียงแต่ใช้พื้นที่น้อยลงเท่านั้น

**ต่างจากระบบบีบอัดในตัวของ Windows รุ่นเก่าอย่างไร?**

หลักการคล้ายกับระบบบีบอัดเดิมของ Windows (คลิกขวา > คุณสมบัติ > บีบอัดเนื้อหาเพื่อประหยัดพื้นที่) แต่อัลกอริทึมใหม่ที่มากับ Windows 10 ขึ้นไปดีกว่ามาก ให้อัตราการบีบอัดสูงกว่าโดยแทบไม่กระทบประสิทธิภาพ [อ่านข้อมูลเพิ่มเติมได้ที่นี่](<https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx>)

## การติดตั้ง

<a href="https://github.com/IridiumIO/CompactGUI/releases"><img alt="ดาวน์โหลดจาก GitHub" src="https://img.shields.io/badge/%E0%B8%94%E0%B8%B2%E0%B8%A7%E0%B8%99%E0%B9%8C%E0%B9%82%E0%B8%AB%E0%B8%A5%E0%B8%94%20%E0%B8%88%E0%B8%B2%E0%B8%81%20GitHub-steelblue?style=for-the-badge&logo=github&link=https%3A%2F%2Fgithub.com%2FIridiumIO%2FCompactGUI%2Freleases"></a>

หรือติดตั้งผ่าน WinGet:

```powershell
winget install CompactGUI
```

## การใช้งาน

ใช้เครื่องมือนี้เพื่อบีบอัดโฟลเดอร์โดยที่ยังเปิดใช้งาน/รันไฟล์ข้างในได้ตามปกติ:

- ลดขนาดเกม (เช่น ARK: Survival Evolved: 169 GB → 91.2 GB)
- ลดขนาดโปรแกรม (เช่น Adobe Photoshop: 1.71 GB → 886 MB)
- บีบอัดโฟลเดอร์อื่น ๆ บนเครื่องของคุณได้เช่นกัน

## ฟีเจอร์เพิ่มเติม

- แสดงความคืบหน้าและสถิติการบีบอัดแบบเห็นภาพ
- รายการนามสกุลไฟล์ที่บีบอัดได้น้อยให้ข้ามได้ตามต้องการ ปรับแยกรายโฟลเดอร์ได้
- ประมาณการบีบอัดจากผลลัพธ์ที่ชุมชนส่งเข้ามากว่า 100,000 รายการ ให้ข้อมูลที่แม่นยำสำหรับหลายเกมบน Steam
  - ตามจริงมีมากกว่านั้นมาก แต่ผู้พัฒนาไม่ทันสังเกตว่า Google Forms เก็บได้แค่ 100,000 รายการ ทำให้ข้อมูลจำนวนมากหายไป
  - เกมนอก Steam ก็ยังใช้การประมาณแบบอัลกอริทึมได้ ซึ่งให้แนวโน้มที่ค่อนข้างใช้ได้จริง
  - ถ้าอยากช่วยเพิ่มข้อมูล สามารถส่งผลการบีบอัดของเกม Steam เข้าฐานข้อมูลออนไลน์ได้จากใน CompactGUI โดยตรง
- เชื่อมกับเมนูคลิกขวาของ Windows Explorer เพื่อใช้งานง่ายขึ้น
- วิเคราะห์สถานะการบีบอัดของโฟลเดอร์ที่มีอยู่แล้ว
- Background Watcher — เฝ้าดูโฟลเดอร์และติดตามการเปลี่ยนแปลง (เช่นเกม Steam อัปเดต) และบีบอัดกลับให้เองในเบื้องหลังโดยอัตโนมัติ

<h4 align="center"><b>ดูรายชื่อเกมที่ผ่านการทดสอบจากผลลัพธ์กว่า 100,000 รายการได้ที่ <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a></b></h4>
<p>&nbsp;</p>

## ข้อควรระวัง

**ไม่ควรใช้เครื่องมือนี้กับเกมที่ใช้ DirectStorage บน Windows 11**

DirectStorage คือ API ที่ให้เกมโหลดไฟล์จาก SSD ส่งเข้า GPU ได้โดยตรงโดยไม่ผ่าน CPU ไฟล์ที่ถูกบีบอัดจะต้องคลายออกก่อนส่งต่อให้ GPU ซึ่งจะทำให้ข้อด้านประสิทธิภาพหายไป

## เบื้องหลัง

Windows 10 เพิ่มเครื่องมือที่ใช้งานได้จริงแต่คนไม่ค่อยรู้จักชื่อ `compact.exe` ซึ่งบีบอัดโฟลเดอร์และไฟล์ลงดิสก์ แล้วคลายออกอัตโนมัติขณะใช้งาน CPU ยุคใหม่รับภาระเพิ่มนี้ได้แทบไม่รู้สึก (ผู้พัฒนาทดสอบกับ Intel Core i3-370M ปี 2010 ก็ผลกระทบน้อยมาก) พื้นที่ที่ประหยัดได้มีประโยชน์ที่สุดกับผู้ที่ใช้ SSD ขนาดเล็ก

เมื่อโฟลเดอร์โปรแกรมและเกมย่อขนาดลงได้ถึง 60% ยังช่วยให้โหลดเร็วขึ้นได้อีกด้วย โดยเฉพาะบน HDD ที่ช้า

ข้อมูลเพิ่มเติมเกี่ยวกับฟังก์ชันในตัวของ Windows ดูได้[ที่นี่](https://technet.microsoft.com/library/bb490884.aspx)และ[ที่นี่](<https://msdn.microsoft.com/library/windows/desktop/hh920921(v=vs.85).aspx>) หรือพิมพ์ `compact /q` ในบรรทัดคำสั่ง

CompactGUI ออกแบบมาให้บีบอัดเฉพาะโฟลเดอร์และไฟล์เท่านั้น ไม่สามารถแก้ไขทั้งไดรฟ์หรือ Windows ทั้งระบบได้ — ผู้ที่ต้องการฟังก์ชันนั้นควรใช้คำสั่ง `compact /compactOS` จากบรรทัดคำสั่งแทน

การบีบอัดนี้โปร่งใสอย่างสมบูรณ์ โปรแกรม เกม และไฟล์เข้าถึงได้ตามปกติ และแสดงใน Explorer เหมือนเดิม — จะคลายออกใน RAM ขณะใช้งาน โดยยังคงบีบอัดอยู่บนดิสก์

## โหมดการบีบอัด

ค่าเริ่มต้นของโปรแกรมคืออัลกอริทึม `XPRESS8K` ซึ่งสมดุลระหว่างความเร็วกับขนาดที่ลดลง ค่าเริ่มต้นของ Windows เองคือ `XPRESS4K` ซึ่งเร็วกว่าแต่บีบอัดได้น้อยกว่า

โหมดการบีบอัดเสริมที่เลือกได้:

| อัลกอริทึม | จุดเด่น | คำอธิบาย |
| :---------- | :------------------- | :----------- |
| XPRESS4K | เร็วที่สุด แต่บีบอัดได้อ่อนที่สุด | เหมาะกับไฟล์เกมที่ต้องการความเร็วในการอ่านสูงมาก สามารถรักษาประสิทธิภาพไว้ได้ในขณะที่บีบอัด |
| XPRESS8K | สมดุลระหว่างความเร็วและการบีบอัด | สมดุลที่ดีระหว่างความเร็วในการบีบอัดกับอัตราส่วนการบีบอัด |
| XPRESS16K | ช้ากว่า แต่บีบอัดได้แรงกว่า | เหมาะกับกรณีที่พื้นที่จำกัดและความเร็วการโหลดไม่ใช่เรื่องสำคัญ |
| LZX | ช้าที่สุด แต่บีบอัดได้แรงที่สุด | เหมาะกับไฟล์จัดเก็บ ข้อมูลสำรอง หรือข้อมูลที่ไม่ค่อยได้เปิดใช้ |

---

### ชอบโปรเจกต์นี้ไหม?

โปรดพิจารณาสนับสนุนผู้พัฒนาด้วยการทิปบน Ko-Fi :)

<p align="center"><a href="https://ko-fi.com/iridiumio" target="_blank"><img height="42" style="border:0px;height:42px;" src="https://cdn.ko-fi.com/cdn/kofi3.png?v=3" border="0" alt="สนับสนุนกาแฟให้ผู้พัฒนาที่ ko-fi.com" /></a></p>
