#include <stdio.h>
#include <string.h>
#include <math.h>
#include <algorithm>
#include <libraw.h>
#include <time.h>
#include <sys/utime.h>
#include <string>
#include <winsock2.h>
#include "LibRawWrapper.h"
#include <iostream>

#pragma comment( lib, "ws2_32.lib")

unsigned char *processRawImage(void *buffer, int size)
{
	bool manual_process = false;

	LibRaw RawProcessor;
	RawProcessor.imgdata.params.output_bps = 16;

	libraw_processed_image_t *proc_img = NULL;

	if (manual_process) {
		////char outfn[1024] = "./Results/out_manual.tiff";


		//// Read RAW image
		//RawProcessor.open_file(av);
		//// Decode Bayer data (no demosaic, no interpolation); decode EXIF data
		//RawProcessor.unpack();

		//// Substract black level + scale on 14-bits (not 16-bit because white balance can overflow some values)
		//ushort *raw_image = new ushort[RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width];

		//unsigned max = 0, min = RawProcessor.imgdata.color.black, scale;
		//for (int j = 0; j < RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width; j++) {
		//	if (max < RawProcessor.imgdata.rawdata.raw_image[j])
		//		max = RawProcessor.imgdata.rawdata.raw_image[j];
		//	if (min > RawProcessor.imgdata.rawdata.raw_image[j])
		//		min = RawProcessor.imgdata.rawdata.raw_image[j];
		//}

		//if (max > 0 && max < 1 << 14)
		//{
		//	scale = ((1 << 14) - 1) / (max - min);
		//	for (int j = 0; j < RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width; j++) {
		//		RawProcessor.imgdata.rawdata.raw_image[j] -= min;
		//		RawProcessor.imgdata.rawdata.raw_image[j] *= scale;
		//	}
		//}


		//// White balance (with camera presets 3000K)
		//float *WB_mul = { RawProcessor.imgdata.rawdata.color.WBCT_Coeffs[12] };

		//int row, col;
		//for (row = 0; row < RawProcessor.imgdata.sizes.height; row++)
		//	for (col = 0; col < RawProcessor.imgdata.sizes.width; col++)
		//		RawProcessor.imgdata.rawdata.raw_image[(row + RawProcessor.imgdata.sizes.top_margin) * RawProcessor.imgdata.sizes.raw_pitch / 2 + (col + RawProcessor.imgdata.sizes.left_margin)] *= WB_mul[FC(row, col, RawProcessor.imgdata.idata.filters) + 1];

		//// Scale on 16-bit
		//max = 0, min = RawProcessor.imgdata.color.black;
		//for (int j = 0; j < RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width; j++) {
		//	if (max < RawProcessor.imgdata.rawdata.raw_image[j])
		//		max = RawProcessor.imgdata.rawdata.raw_image[j];
		//	if (min > RawProcessor.imgdata.rawdata.raw_image[j])
		//		min = RawProcessor.imgdata.rawdata.raw_image[j];
		//}

		//if (max > 0 && max < 1 << 16)
		//{
		//	scale = ((1 << 16) - 1) / (max);
		//	for (int j = 0; j < RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width; j++) {
		//		RawProcessor.imgdata.rawdata.raw_image[j] *= scale;
		//	}
		//}

		//// Create OpenCV Mat with bayer raw data
		//Mat mat16uc1_bayer(RawProcessor.imgdata.sizes.raw_height, RawProcessor.imgdata.sizes.raw_width, CV_16UC1, RawProcessor.imgdata.rawdata.raw_image);

		//// Complete demosaicing on bayer and put in uint 16 3Colors
		//Mat mat16uc3_rgb(RawProcessor.imgdata.sizes.raw_height, RawProcessor.imgdata.sizes.raw_width, CV_16UC3);
		//demosaicing(mat16uc1_bayer, mat16uc3_rgb, COLOR_BayerRG2RGB, 3);

		//// Apply gamma correction of standard 1/2.2
		//bool gamma = true;
		//if (gamma) {
		//	// create gamma correction lut and apply
		//	gamma_curve(RawProcessor.imgdata.color.curve, RawProcessor.imgdata.params.gamm, 0xffff);
		//	for (int j = 0; j < RawProcessor.imgdata.sizes.raw_height * RawProcessor.imgdata.sizes.raw_width * 3; j++)
		//		((ushort*)mat16uc3_rgb.data)[j] =
		//		RawProcessor.imgdata.color.curve[((ushort*)mat16uc3_rgb.data)[j]];
		//}

		//// write the 16-bit .tiff
		//imwrite("./results/out_manual.tiff", mat16uc3_rgb);
		//
		//// Release the LibRaw processor & call destructors
		//RawProcessor.recycle();
	}
	else {
	//char outfn[1024] = "./Results/out_dcRaw.tiff";
	char outfn[1024] = "G:/LibRawWrapper/x64/Debug/out_dcRaw.tiff";
		RawProcessor.imgdata.params.output_tiff = 1;
		//RawProcessor.imgdata.params.no_auto_scale = 1;
		//RawProcessor.imgdata.params.no_auto_bright = 1;
		//RawProcessor.imgdata.params.gamm[0] = 1.0;
		//RawProcessor.imgdata.params.gamm[1] = 1.0;
		//RawProcessor.imgdata.params.auto_bright_thr = 0.1;
		RawProcessor.imgdata.params.use_camera_wb = 1;

		// Read RAW image
		//RawProcessor.open_file(av);
		RawProcessor.open_buffer(buffer, size);
		// Decode Bayer data (no demosaic, no interpolation); decode EXIF data
		RawProcessor.unpack();

		// white balance + color interpolation + colorspace conversion
		int ret = RawProcessor.dcraw_process();

		// gamma correction + create 3 component Bitmap
		proc_img = RawProcessor.dcraw_make_mem_image(&ret);

		// Write 16-bit .tiff
		//RawProcessor.dcraw_ppm_tiff_writer(outfn);
		
		// Release the LibRaw processor & call destructors
		RawProcessor.recycle();
		RawProcessor.free_image();
	}
	return proc_img->data;
	//return (12);
}

int extractThumb(void *buffer, int size)
{
	LibRaw RawProcessor;

	libraw_processed_image_t *proc_img = NULL;

	RawProcessor.open_buffer(buffer, size);
	//RawProcessor.open_file("G:/LibRawTester/LibRawTester/Resources/batim.cr3");
	RawProcessor.unpack_thumb();

	proc_img = RawProcessor.dcraw_make_mem_thumb();

	unsigned char *buff = new unsigned char[5000000];

	int s = get_jpeg_buffer(buff, &RawProcessor.imgdata);
	
	memcpy(buffer, buff, s);

	RawProcessor.recycle();
	RawProcessor.free_image();

	delete[] buff;

	return s;
}

int extractThumbFromFile(void *buffer, const char *path)
{
	LibRaw RawProcessor;

	libraw_processed_image_t *proc_img = NULL;

	RawProcessor.open_file(path);
	RawProcessor.unpack_thumb();

	proc_img = RawProcessor.dcraw_make_mem_thumb();

	unsigned char *buff = new unsigned char[5000000];

	int s = get_jpeg_buffer(buff, &RawProcessor.imgdata);

	memcpy(buffer, buff, s);

	RawProcessor.recycle();
	RawProcessor.free_image();

	delete[] buff;

	return s;
}

int main()
{
	//char av[] ="./Resources/eye.cr3";
	//char av[] ="./Resources/batim.cr3";
	char av[] = "G:/LibRawWrapper/LibRawWrapper/Resources/batim.cr3";
	//char av[] ="./Resources/fleur.cr2";

	//int whew = processRawImage(10);
	
	extractThumb(av, 30);

	return 0;
}

int get_jpeg_buffer(unsigned char *buffer, libraw_data_t *imgdata)
{
	ushort exif[5];
	struct tiff_hdr th;
	int count = 0;
	std::cout << count;
	memset(buffer + count, 0xff, sizeof(unsigned char));
	count += sizeof(unsigned char);
	std::cout << count;
	memset(buffer + count, 0xd8, sizeof(unsigned char));
	count += sizeof(unsigned char);
	std::cout << count;
	std::cout << *buffer;
	if (strcmp(imgdata->thumbnail.thumb + 6, "Exif"))
	{
		memcpy(exif, "\xff\xe1  Exif\0\0", 10);
		exif[1] = htons(8 + sizeof th);
		memcpy(buffer + count, exif, sizeof exif);
		count += sizeof exif;
		tiff_head(imgdata->thumbnail.twidth, imgdata->thumbnail.theight, &th);
		memcpy(buffer + count, &th, sizeof th);
		count += sizeof th;
	}
	memcpy(buffer + count, imgdata->thumbnail.thumb + 2, imgdata->thumbnail.tlength - 2);
	count += imgdata->thumbnail.tlength - 2;
	return count;
}

int FC(int row, int col, unsigned int filters)
{
	return (filters >> (((row << 1 & 14) | (col & 1)) << 1) & 3);
}

void tiff_set(ushort *ntag, ushort tag, ushort type, int count, int val)
{
	struct libraw_tiff_tag *tt;
	int c;

	tt = (struct libraw_tiff_tag *)(ntag + 1) + (*ntag)++;
	tt->tag = tag;
	tt->type = type;
	tt->count = count;
	if (type < 3 && count <= 4)
		for (c = 0; c < 4; c++)
			tt->val.c[c] = val >> (c << 3);
	else if (type == 3 && count <= 2)
		for (c = 0; c < 2; c++)
			tt->val.s[c] = val >> (c << 4);
	else
		tt->val.i = val;
}
#define TOFF(ptr) ((char *)(&(ptr)) - (char *)th)

void tiff_head(int width, int height, struct tiff_hdr *th)
{
	int c;
	time_t timestamp = time(NULL);
	struct tm *t;


	memset(th, 0, sizeof *th);
	th->t_order = htonl(0x4d4d4949) >> 16;
	th->magic = 42;
	th->ifd = 10;
	tiff_set(&th->ntag, 254, 4, 1, 0);
	tiff_set(&th->ntag, 256, 4, 1, width);
	tiff_set(&th->ntag, 257, 4, 1, height);
	tiff_set(&th->ntag, 258, 3, 1, 16);
	for (c = 0; c < 4; c++)
		th->bps[c] = 16;
	tiff_set(&th->ntag, 259, 3, 1, 1);
	tiff_set(&th->ntag, 262, 3, 1, 1);
	tiff_set(&th->ntag, 273, 4, 1, sizeof *th);
	tiff_set(&th->ntag, 277, 3, 1, 1);
	tiff_set(&th->ntag, 278, 4, 1, height);
	tiff_set(&th->ntag, 279, 4, 1, height * width * 2);
	tiff_set(&th->ntag, 282, 5, 1, TOFF(th->rat[0]));
	tiff_set(&th->ntag, 283, 5, 1, TOFF(th->rat[2]));
	tiff_set(&th->ntag, 284, 3, 1, 1);
	tiff_set(&th->ntag, 296, 3, 1, 2);
	tiff_set(&th->ntag, 306, 2, 20, TOFF(th->date));
	th->rat[0] = th->rat[2] = 300;
	th->rat[1] = th->rat[3] = 1;
	t = localtime(&timestamp);
	if (t)
		sprintf_s(th->date, "%04d:%02d:%02d %02d:%02d:%02d", t->tm_year + 1900,
			t->tm_mon + 1, t->tm_mday, t->tm_hour, t->tm_min, t->tm_sec);
}



/*
void tiff_head_full(struct tiff_hdr *th, int full)
{
	int c, psize = 0;
	struct tm *t;

	memset(th, 0, sizeof *th);
	th->t_order = htonl(0x4d4d4949) >> 16;
	th->magic = 42;
	th->ifd = 10;
	th->rat[0] = th->rat[2] = 300;
	th->rat[1] = th->rat[3] = 1;
	FORC(6) th->rat[4 + c] = 1000000;
	th->rat[4] *= shutter;
	th->rat[6] *= aperture;
	th->rat[8] *= focal_len;
	strncpy(th->t_desc, desc, 512);
	strncpy(th->t_make, make, 64);
	strncpy(th->t_model, model, 64);
	strcpy(th->soft, "dcraw v" DCRAW_VERSION);
	t = localtime(&timestamp);
	sprintf(th->date, "%04d:%02d:%02d %02d:%02d:%02d", t->tm_year + 1900,
		t->tm_mon + 1, t->tm_mday, t->tm_hour, t->tm_min, t->tm_sec);
	strncpy(th->t_artist, artist, 64);
	if (full)
	{
		tiff_set(th, &th->ntag, 254, 4, 1, 0);
		tiff_set(th, &th->ntag, 256, 4, 1, width);
		tiff_set(th, &th->ntag, 257, 4, 1, height);
		tiff_set(th, &th->ntag, 258, 3, colors, output_bps);
		if (colors > 2)
			th->tag[th->ntag - 1].val.i = TOFF(th->bps);
		FORC4 th->bps[c] = output_bps;
		tiff_set(th, &th->ntag, 259, 3, 1, 1);
		tiff_set(th, &th->ntag, 262, 3, 1, 1 + (colors > 1));
	}
	tiff_set(th, &th->ntag, 270, 2, 512, TOFF(th->t_desc));
	tiff_set(th, &th->ntag, 271, 2, 64, TOFF(th->t_make));
	tiff_set(th, &th->ntag, 272, 2, 64, TOFF(th->t_model));
	if (full)
	{
		if (oprof)
			psize = ntohl(oprof[0]);
		tiff_set(th, &th->ntag, 273, 4, 1, sizeof *th + psize);
		tiff_set(th, &th->ntag, 277, 3, 1, colors);
		tiff_set(th, &th->ntag, 278, 4, 1, height);
		tiff_set(th, &th->ntag, 279, 4, 1,
			height * width * colors * output_bps / 8);
	}
	else
		tiff_set(th, &th->ntag, 274, 3, 1, "12435867"[flip] - '0');
	tiff_set(th, &th->ntag, 282, 5, 1, TOFF(th->rat[0]));
	tiff_set(th, &th->ntag, 283, 5, 1, TOFF(th->rat[2]));
	tiff_set(th, &th->ntag, 284, 3, 1, 1);
	tiff_set(th, &th->ntag, 296, 3, 1, 2);
	tiff_set(th, &th->ntag, 305, 2, 32, TOFF(th->soft));
	tiff_set(th, &th->ntag, 306, 2, 20, TOFF(th->date));
	tiff_set(th, &th->ntag, 315, 2, 64, TOFF(th->t_artist));
	tiff_set(th, &th->ntag, 34665, 4, 1, TOFF(th->nexif));
	if (psize)
		tiff_set(th, &th->ntag, 34675, 7, psize, sizeof *th);
	tiff_set(th, &th->nexif, 33434, 5, 1, TOFF(th->rat[4]));
	tiff_set(th, &th->nexif, 33437, 5, 1, TOFF(th->rat[6]));
	tiff_set(th, &th->nexif, 34855, 3, 1, iso_speed);
	tiff_set(th, &th->nexif, 37386, 5, 1, TOFF(th->rat[8]));
	if (gpsdata[1])
	{
		tiff_set(th, &th->ntag, 34853, 4, 1, TOFF(th->ngps));
		tiff_set(th, &th->ngps, 0, 1, 4, 0x202);
		tiff_set(th, &th->ngps, 1, 2, 2, gpsdata[29]);
		tiff_set(th, &th->ngps, 2, 5, 3, TOFF(th->gps[0]));
		tiff_set(th, &th->ngps, 3, 2, 2, gpsdata[30]);
		tiff_set(th, &th->ngps, 4, 5, 3, TOFF(th->gps[6]));
		tiff_set(th, &th->ngps, 5, 1, 1, gpsdata[31]);
		tiff_set(th, &th->ngps, 6, 5, 1, TOFF(th->gps[18]));
		tiff_set(th, &th->ngps, 7, 5, 3, TOFF(th->gps[12]));
		tiff_set(th, &th->ngps, 18, 2, 12, TOFF(th->gps[20]));
		tiff_set(th, &th->ngps, 29, 2, 12, TOFF(th->gps[23]));
		memcpy(th->gps, gpsdata, sizeof th->gps);
	}
}
*/


/* 
	Grayscale image - 1 channel bitmap - bayer images
*/
void write_tiff(int width, int height, unsigned short *bitmap, const char *fn)
{
	struct tiff_hdr th;
	FILE *ofp = fopen(fn, "wb");
	
	if (!ofp)
		return;
	tiff_head(width, height, &th);
	fwrite(&th, sizeof th, 1, ofp);
	fwrite(bitmap, 2, width * height, ofp);
	fclose(ofp);
}

#define SQR(x) ((x) * (x))

void gamma_curve(unsigned short *curve, double *gamm, int imax)
{

	int mode = 2;
	int i;
	double g[6], bnd[2] = { 0, 0 }, r;

	g[0] = gamm[0];
	g[1] = gamm[1];
	g[2] = g[3] = g[4] = 0;
	bnd[g[1] >= 1] = 1;
	if (g[1] && (g[1] - 1) * (g[0] - 1) <= 0)
	{
		for (i = 0; i < 48; i++)
		{
			g[2] = (bnd[0] + bnd[1]) / 2;
			if (g[0])
				bnd[(pow(g[2] / g[1], -g[0]) - 1) / g[0] - 1 / g[2] > -1] = g[2];
			else
				bnd[g[2] / exp(1 - 1 / g[2]) < g[1]] = g[2];
		}
		g[3] = g[2] / g[1];
		if (g[0])
			g[4] = g[2] * (1 / g[0] - 1);
	}
	if (g[0])
		g[5] = 1 / (g[1] * SQR(g[3]) / 2 - g[4] * (1 - g[3]) +
		(1 - pow(g[3], 1 + g[0])) * (1 + g[4]) / (1 + g[0])) -
		1;
	else
		g[5] = 1 / (g[1] * SQR(g[3]) / 2 + 1 - g[2] - g[3] -
			g[2] * g[3] * (log(g[3]) - 1)) -
		1;

	memcpy(gamm, g, sizeof gamm);

	for (i = 0; i < 0x10000; i++)
	{
		curve[i] = 0xffff;
		if ((r = (double)i / imax) < 1)
			curve[i] =
			0x10000 *
			(mode ? (r < g[3] ? r * g[1]
				: (g[0] ? pow(r, g[0]) * (1 + g[4]) - g[4]
					: log(r) * g[2] + 1))
				: (r < g[2] ? r / g[1]
					: (g[0] ? pow((r + g[4]) / (1 + g[4]), 1 / g[0])
						: exp((r - 1) / g[2]))));
	}
}
