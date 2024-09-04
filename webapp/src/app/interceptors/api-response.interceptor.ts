import { HttpErrorResponse, HttpEventType, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

export const ApiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  const toaster = inject(ToastrService);
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status == HttpStatusCode.Unauthorized) {
        toaster.error("UnAuthorized access");
      }
      else {
        toaster.error("somethign went wrong please try again later!");
      }
      throw error;
    })
  );
};
