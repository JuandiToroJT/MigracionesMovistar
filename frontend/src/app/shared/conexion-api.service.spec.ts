import { TestBed } from '@angular/core/testing';

import { ConexionApiService } from './conexion-api.service';

describe('ConexionApiService', () => {
  let service: ConexionApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConexionApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
