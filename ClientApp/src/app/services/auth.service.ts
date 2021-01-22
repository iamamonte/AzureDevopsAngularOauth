import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Configuration } from '../util/config';
import { Observable } from 'rxjs';


@Injectable()
export class AuthService {
  constructor(private httpService: HttpClient, private config: Configuration) {
  }

  /**
   *Success status should include a JWT
   * @param username
   * @param password
   */
  login(): Observable<string> {
    return this.httpService.get(`https://localhost:44341/api/oauth/login`, {"responseType": "text"});
  }
}
