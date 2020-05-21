#pragma once

extern "C"
{
	__declspec(dllexport) 
		unsigned char *processRawImage(void *buffer, int size);

	__declspec(dllexport)
		int extractThumb(void *buffer, int size);

	__declspec(dllexport)
		int extractThumbFromFile(void *buffer, const char *path);
}


int get_jpeg_buffer(unsigned char *buffer, libraw_data_t *imgdata);
void write_tiff(int width, int height, unsigned short *bitmap, const char *basename);
void tiff_head(int width, int height, struct tiff_hdr *th);
void gamma_curve(unsigned short *curve, double *gamm, int imax);
int FC(int row, int col, unsigned int filters);