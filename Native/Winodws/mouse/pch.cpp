// pch.cpp: 与预编译标头对应的源文件

#include "pch.h"
#include "windows.h"
// 当使用预编译的头时，需要使用此源文件，编译才能成功。


void Move2Left() {
	POINT point;
	LPPOINT p = &point;
	GetCursorPos(p);
	SetCursorPos(p->x - 1, p->y);
}

void Move2Right() {
	POINT point;
	LPPOINT p = &point;
	GetCursorPos(p);
	SetCursorPos(p->x + 1, p->y);
}

void Move2Top() {
	POINT point;
	LPPOINT p = &point;
	GetCursorPos(p);
	SetCursorPos(p->x, p->y - 1);
}

void Move2Bottom() {
	POINT point;
	LPPOINT p = &point;
	GetCursorPos(p);
	SetCursorPos(p->x, p->y + 1);
}